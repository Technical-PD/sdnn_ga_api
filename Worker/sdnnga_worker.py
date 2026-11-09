"""
SDNN-GA локальний воркер.

Замінює CreateModelService та FitModelService (RabbitMQ) одним процесом, який
запускає десктопний застосунок. Протокол — JSON Lines через stdin/stdout:

  запит:     {"id": "...", "type": "create" | "fit" | "ping", "payload": {...}}
  відповідь: {"id": "...", "ok": true, "result": ...}
             {"id": "...", "ok": false, "error": "..."}

Після імпорту TensorFlow воркер надсилає {"type": "ready", ...}.
Весь інший вивід (логи Keras/TensorFlow, print) іде в stderr, щоб не ламати протокол.
"""

import base64
import io
import json
import math
import os
import sys
import tempfile
import traceback

# Протокольний канал — справжній stdout. Все, що друкується через print(), іде в stderr.
_protocol_out = sys.stdout
sys.stdout = sys.stderr

for _stream in (sys.stdin, _protocol_out, sys.stderr):
    try:
        _stream.reconfigure(encoding="utf-8")
    except (AttributeError, ValueError):
        pass

os.environ.setdefault("TF_CPP_MIN_LOG_LEVEL", "2")


def send(message):
    _protocol_out.write(json.dumps(message) + "\n")
    _protocol_out.flush()


def log(text):
    print(text, file=sys.stderr, flush=True)


try:
    import numpy as np
    import pandas as pd
    import tensorflow as tf
    from tensorflow.keras.layers import Dense, Input
    from tensorflow.keras.models import Sequential
except Exception as import_error:  # noqa: BLE001
    send({"type": "fatal", "error": (
        f"Python at '{sys.executable}' (version {sys.version.split()[0]}) cannot import the ML libraries: {import_error!r}. "
        f"Use a Python 3.9-3.13 environment with the packages from Worker/requirements.txt "
        f"and point 'Worker:PythonPath' at its python.exe."
    )})
    sys.exit(1)


def create_model(model_config):
    """ModelConfig (.NET) -> Keras model JSON."""
    model = Sequential()

    # Додавання вхідного шару
    model.add(Input(shape=model_config["InputShape"], name="input"))

    # Додавання внутрішніх шарів
    for layer_config in model_config["InternalLayers"]:
        model.add(Dense(units=layer_config["NeuronsCount"],
                        use_bias=layer_config["UseBias"],
                        activation=layer_config["ActivationFunc"]))

    # Додавання вихідного шару
    output_layer = model_config["OutputLayer"]
    model.add(Dense(units=output_layer["NeuronsCount"],
                    use_bias=output_layer["UseBias"],
                    activation=output_layer["ActivationFunc"]))

    return model.to_json()


def _to_builtin(value):
    """
    numpy-типи з history.history -> звичайні float, щоб json.dumps їх прийняв.
    NaN/Infinity (модель "розійшлася") -> None: стандартний JSON їх не підтримує, і .NET не зміг би розібрати відповідь.
    """
    if isinstance(value, dict):
        return {k: _to_builtin(v) for k, v in value.items()}
    if isinstance(value, (list, tuple)):
        return [_to_builtin(v) for v in value]
    if isinstance(value, (np.floating, np.integer)):
        value = value.item()
    if isinstance(value, float) and not math.isfinite(value):
        return None
    return value


def fit_model(cfg):
    """FitModelJobConfig (.NET) -> {Loss, Accuracy, History, Weights(base64)}."""
    x_all = pd.read_csv(io.StringIO(cfg["XTrain"]), header=0).values.astype(float)
    y_all = pd.read_csv(io.StringIO(cfg["YTrain"]), header=0).values.astype(float)

    if len(x_all) != len(y_all):
        raise ValueError(f"X and Y have different row counts ({len(x_all)} vs {len(y_all)}).")

    # Перемішування обох масивів однаковим чином
    indices = np.arange(len(x_all))
    np.random.shuffle(indices)
    x_data = x_all[indices]
    y_data = y_all[indices]

    train_x, train_y = x_data, y_data
    val_x = val_y = None

    # Train-validation split
    if cfg["IsLearnWithValidation"]:
        split_index = int(len(x_data) * 0.75)
        train_x, val_x = x_data[:split_index], x_data[split_index:]
        train_y, val_y = y_data[:split_index], y_data[split_index:]

    callbacks = []
    if cfg["UseEarlyStopping"]:
        callbacks.append(tf.keras.callbacks.EarlyStopping(
            min_delta=cfg["MinDelta"],
            patience=cfg["Patience"],
            monitor="val_loss" if cfg["IsLearnWithValidation"] else "loss",
        ))

    model = tf.keras.models.model_from_json(cfg["ModelConfigJson"])
    model.compile(optimizer=cfg["Optimizer"], loss=cfg["LossFunc"], metrics=["accuracy"])

    history = model.fit(
        x=train_x,
        y=train_y,
        validation_data=(val_x, val_y) if val_x is not None else None,
        epochs=cfg["Epochs"],
        batch_size=cfg["BatchSize"],
        callbacks=callbacks,
        verbose=0,
    )

    eval_x, eval_y = (val_x, val_y) if cfg["IsLearnWithValidation"] else (train_x, train_y)
    loss, accuracy = model.evaluate(x=eval_x, y=eval_y, verbose=0)

    if not (math.isfinite(loss) and math.isfinite(accuracy)):
        raise ValueError(f"Training diverged: loss={loss}, accuracy={accuracy}.")

    fd, weights_file = tempfile.mkstemp(suffix=".weights.h5")
    os.close(fd)
    try:
        model.save_weights(weights_file)
        with open(weights_file, "rb") as f:
            weights_b64 = base64.b64encode(f.read()).decode("ascii")
    finally:
        if os.path.exists(weights_file):
            os.remove(weights_file)

    log(f"Fit finished: loss={loss:.5f}, accuracy={accuracy:.5f}, epochs={len(history.history.get('loss', []))}")

    return {
        "Loss": round(float(loss), 5),
        "Accuracy": round(float(accuracy), 5),
        "History": _to_builtin(history.history),
        "Weights": weights_b64,
    }


HANDLERS = {
    "ping": lambda payload: "pong",
    "create": create_model,
    "fit": fit_model,
}


def main():
    send({"type": "ready", "tensorflow": tf.__version__, "python": sys.version.split()[0]})
    log(f"[worker] ready (TensorFlow {tf.__version__})")

    # Кінець stdin означає, що десктопний застосунок завершився.
    for line in sys.stdin:
        line = line.strip()
        if not line:
            continue

        request_id = None
        try:
            request = json.loads(line)
            request_id = request.get("id")
            handler = HANDLERS.get(request.get("type"))
            if handler is None:
                raise ValueError(f"Unknown request type: {request.get('type')!r}")
            log(f"[worker] {request['type']} {request_id}")
            send({"id": request_id, "ok": True, "result": handler(request.get("payload"))})
        except Exception as error:  # noqa: BLE001
            traceback.print_exc(file=sys.stderr)
            send({"id": request_id, "ok": False, "error": f"{type(error).__name__}: {error}"})


if __name__ == "__main__":
    main()
