import pika
import json

def process_message(message):
    # Тут обробка повідомлення (просто повернемо те ж повідомлення у відповідь)
    return f"Processed: {message}"

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
    message = body.decode()
    print(f" [x] Received: {message}")

    response = process_message(message)

    # Надсилаємо відповідь у чергу, звідки очікує клієнт
    ch.basic_publish(exchange='',
                     routing_key=properties.reply_to,
                     properties=pika.BasicProperties(correlation_id=properties.correlation_id),
                     body=response)
    ch.basic_ack(delivery_tag=method.delivery_tag)

channel.basic_consume(queue=request_queue, on_message_callback=on_request)
print(" [*] Waiting for messages.")
channel.start_consuming()