import pika
import numpy as np
import tensorflow as tf
import json
import io

def fit_model(model_config_json):
    # Prepare training data
    nd_array_x = np.array(model_config_json['XTrain']).reshape(model_config_json['CountOfLines'], model_config_json['CountOfInputs'])
    nd_array_y = np.array(model_config_json['YTrain']).reshape(model_config_json['CountOfLines'], model_config_json['CountOfOutputs'])

    train_x = nd_array_x
    train_y = nd_array_y
    val_x = None
    val_y = None
    callbacks = []

    # Callbacks for training
    if model_config_json['UseEarlyStopping']:
        callbacks.append(
            tf.keras.callbacks.EarlyStopping(
                min_delta=model_config_json['MinDelta'],
                patience=model_config_json['Patience'],
                monitor='val_loss' if model_config_json['IsLearnWithValidation'] else 'loss'
            )
        )

    # Train-validation split
    val_split_coefficient = 0.75
    if model_config_json['IsLearnWithValidation']:
        split_index = int(len(nd_array_x) * val_split_coefficient)
        train_x, val_x = nd_array_x[:split_index], nd_array_x[split_index:]
        train_y, val_y = nd_array_y[:split_index], nd_array_y[split_index:]

    # Model from JSON
    model_json = model_config_json["ModelConfigJson"]
    model_config = json.loads(model_json)
    model = tf.keras.models.model_from_json(json.dumps(model_config))

    # Compile model
    model.compile(
        optimizer=model_config_json['Optimizer'],
        loss=model_config_json['LossFunc'],
        metrics=['accuracy']
    )

    # Train model
    history = model.fit(
        x=train_x,
        y=train_y,
        validation_data=(val_x, val_y) if val_x is not None and val_y is not None else None,
        epochs=model_config_json['Epochs'],
        batch_size=model_config_json['BatchSize'],
        callbacks=callbacks
    )

    # Evaluate model
    result = model.evaluate(
        x=val_x if model_config_json['IsLearnWithValidation'] else train_x,
        y=val_y if model_config_json['IsLearnWithValidation'] else train_y
    )


    weights_io = io.BytesIO()
    model.save_weights(weights_io)
    weights_binary = weights_io.getvalue()

    fit_history = {
        'Loss': round(result[0], 5),
        'Accuracy': round(result[1], 5),
        'History': history.history,
        'Epochs': history.epoch,
        'Weights': weights_binary
    }

    return fit_history

def process_message(message):
    # Тут обробка повідомлення (просто повернемо те ж повідомлення у відповідь)
    return message

# Налаштування RabbitMQ
connection_params = pika.ConnectionParameters('rabbitmq-service')  # Перевірте, що hostname відповідає
connection = pika.BlockingConnection(connection_params)
channel = connection.channel()

# Створення черги
request_queue = 'fit_request_queue'
response_queue = 'fit_response_queue'
channel.queue_declare(queue=request_queue, durable=True)
channel.queue_declare(queue=response_queue, durable=True)

# Обробка повідомлення
def on_request(ch, method, properties, body):
    model_config_str = body.decode()
    print(f" [x] Received: {model_config_str}")

    model_config_json = json.loads(model_config_str)

    json_model = fit_model(model_config_json)

    response = process_message(json_model)

    # Надсилаємо відповідь у чергу, звідки очікує клієнт
    ch.basic_publish(exchange='',
                     routing_key=properties.reply_to,
                     properties=pika.BasicProperties(correlation_id=properties.correlation_id),
                     body=response)
    ch.basic_ack(delivery_tag=method.delivery_tag)

channel.basic_consume(queue=request_queue, on_message_callback=on_request)
channel.basic_qos(prefetch_count=1)
print(" [*] Waiting for messages.")
channel.start_consuming()
