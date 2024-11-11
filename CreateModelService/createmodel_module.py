import pika
import json
from tensorflow.keras.models import Sequential
from tensorflow.keras.layers import Dense, Input

def generate_model(model_config_json):
    # Ініціалізація моделі
    model = Sequential()

    # Додавання вхідного шару
    model.add(Input(shape=model_config_json['InputShape'], name="input"))

    # Додавання внутрішніх шарів
    for layer_config in model_config_json['InternalLayers']:
        model.add(Dense(units=layer_config['NeuronsCount'],
                        use_bias=layer_config['UseBias'],
                        activation=layer_config['ActivationFunc']))

    # Додавання вихідного шару
    output_layer = model_config_json['OutputLayer']
    model.add(Dense(units=output_layer['NeuronsCount'],
                    use_bias=output_layer['UseBias'],
                    activation=output_layer['ActivationFunc']))

    # Конвертація моделі в JSON формат
    json_model = model.to_json()

    return json_model

def process_message(message):
    # Тут обробка повідомлення (просто повернемо те ж повідомлення у відповідь)
    return message

# Налаштування RabbitMQ
connection_params = pika.ConnectionParameters('rabbitmq-service')  # Перевірте, що hostname відповідає
connection = pika.BlockingConnection(connection_params)
channel = connection.channel()

# Створення черги
request_queue = 'request_queue'
response_queue = 'response_queue'
channel.queue_declare(queue=request_queue)
channel.queue_declare(queue=response_queue)

# Обробка повідомлення
def on_request(ch, method, properties, body):
    model_config_str = body.decode()
    print(f" [x] Received: {model_config_str}")

    model_config_json = json.loads(model_config_str)

    json_model = generate_model(model_config_json)

    response = process_message(json_model)

    # Надсилаємо відповідь у чергу, звідки очікує клієнт
    ch.basic_publish(exchange='',
                     routing_key=properties.reply_to,
                     properties=pika.BasicProperties(correlation_id=properties.correlation_id),
                     body=response)
    ch.basic_ack(delivery_tag=method.delivery_tag)

channel.basic_consume(queue=request_queue, on_message_callback=on_request)
print(" [*] Waiting for messages.")
channel.start_consuming()
