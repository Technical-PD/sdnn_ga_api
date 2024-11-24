import pika
import tensorflow as tf
import json
import io
import os
import pandas as pd
import base64
import numpy as np
from azure.storage.blob import BlobServiceClient

connection_string = "DefaultEndpointsProtocol=https;AccountName=sdnngamodelstorage;AccountKey=tihLTqkIHB6IHz8qFt+K9G2T/UnVRtNBVgOImJy3AfX2KOvrJhEsnTppxEFeXWYz1MUeRD3Gdh8V+AStTqeK2w==;EndpointSuffix=core.windows.net"
container_name = "models"

blob_service_client = BlobServiceClient.from_connection_string(connection_string)
container_client = blob_service_client.get_container_client(container_name)

def fit_model(model_config_json):
    # Prepare training data
    nd_array_x_not_shuffled = pd.read_csv(io.StringIO(model_config_json['XTrain']), header=0).values.astype(float)
    nd_array_y_not_shuffled = pd.read_csv(io.StringIO(model_config_json['YTrain']), header=0).values.astype(float)

    # Перевірка розмірів масивів (щоб впевнитися, що вони мають однакову кількість рядків)
    assert len(nd_array_x_not_shuffled) == len(nd_array_y_not_shuffled), "Масиви мають різну кількість рядків!"

    # Створення індексів і перемішування
    indices = np.arange(len(nd_array_x_not_shuffled))
    np.random.shuffle(indices)

    # Перемішування обох масивів однаковим чином
    nd_array_x = nd_array_x_not_shuffled[indices]
    nd_array_y = nd_array_y_not_shuffled[indices]

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

    print("----------------------FIT-STARTED------------------------", flush=True)

    # Train model
    history = model.fit(
        x=train_x,
        y=train_y,
        validation_data=(val_x, val_y) if val_x is not None and val_y is not None else None,
        epochs=model_config_json['Epochs'],
        batch_size=model_config_json['BatchSize'],
        callbacks=callbacks
    )

    print("----------------------FIT-FINISHED------------------------", flush=True)

    # Evaluate model
    result = model.evaluate(
        x=val_x if model_config_json['IsLearnWithValidation'] else train_x,
        y=val_y if model_config_json['IsLearnWithValidation'] else train_y
    )

    weights_file = 'temp.weights.h5'
    model.save_weights(weights_file)

    print("----------------------WEIGHTS-SAVED-LOCALY------------------------", flush=True)

    weigthBlobPath = model_config_json['WeigthPath']
    blob_client = container_client.get_blob_client(weigthBlobPath)

    with open(weights_file, "rb") as data:
        blob_client.upload_blob(data, overwrite=True)

    print("----------------------WEIGHTS-SAVED-IN-BLOB-STORAGE------------------------", flush=True)

    # Remove temporary file if it exists
    if os.path.exists(weights_file):
        os.remove(weights_file)

    print("----------------------WEIGHTS-REMOVED-LOCALY------------------------", flush=True)

    print("----------------------RESPONSE-CREATING------------------------", flush=True)

    fit_history = {
        'Loss': round(result[0], 5),
        'Accuracy': round(result[1], 5),
        'History': history.history,
    }
    print("----------------------RESPONSE-CREATED------------------------", flush=True)

    print(fit_history, flush=True)
    fit_history_json = json.dumps(fit_history)
    return fit_history_json

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

    print("----------------------READY-TO-BE-PUBLISHED------------------------", flush=True)

    # Надсилаємо відповідь у чергу, звідки очікує клієнт
    try:
        ch.basic_publish(exchange='',
            routing_key=properties.reply_to,
            properties=pika.BasicProperties(correlation_id=properties.correlation_id),
            body=response)
        print("----------------------PUBLISHED------------------------", flush=True)
    except Exception as e:
        print(f"Failed to publish message: {e}", flush=True)

    ch.basic_ack(delivery_tag=method.delivery_tag)

channel.basic_consume(queue=request_queue, on_message_callback=on_request)
channel.basic_qos(prefetch_count=1)
print(" [*] Waiting for messages.", flush=True)
channel.start_consuming()
