using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SdnnGa.Model.Infrastructure.Interfaces.RabbitMq;
using System;
using System.Collections.Concurrent;
using System.Text;
using System.Threading.Tasks;

namespace SdnnGa.Infrastructure.RabbitMq;

public class RabbitMqClient : IRabbitMqClient
{
    private readonly string _hostname;
    private readonly string _requestQueue;
    private readonly string _responseQueue;
    private readonly ConnectionFactory _factory;
    private IConnection _connection;
    private IModel _channel;

    // Словник для зберігання очікуваних відповідей за correlationId
    private readonly ConcurrentDictionary<string, TaskCompletionSource<string>> _pendingResponses;

    public RabbitMqClient(string hostname, string requestQueue, string responseQueue)
    {
        _hostname = hostname;
        _requestQueue = requestQueue;
        _responseQueue = responseQueue;
        _factory = new ConnectionFactory() { HostName = _hostname };
        _pendingResponses = new ConcurrentDictionary<string, TaskCompletionSource<string>>();
        InitializeConnection();
    }

    private void InitializeConnection()
    {
        _connection = _factory.CreateConnection();
        _channel = _connection.CreateModel();

        // Налаштування черг як durable для стійкості повідомлень
        _channel.QueueDeclare(queue: _requestQueue, durable: true, exclusive: false, autoDelete: false, arguments: null);
        _channel.QueueDeclare(queue: _responseQueue, durable: true, exclusive: false, autoDelete: false, arguments: null);

        // Ініціалізація споживача для отримання відповідей
        InitializeConsumer();
    }

    private void InitializeConsumer()
    {
        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += (model, ea) =>
        {
            var correlationId = ea.BasicProperties.CorrelationId;
            if (_pendingResponses.TryRemove(correlationId, out var tcs))
            {
                var response = Encoding.UTF8.GetString(ea.Body.ToArray());
                Console.WriteLine($"[x] Received response: {response}");
                tcs.TrySetResult(response);
            }
        };

        // Реєстрація споживача один раз для уникнення дублювання
        _channel.BasicConsume(queue: _responseQueue, autoAck: true, consumer: consumer);
    }

    public async Task<string> SendMessageAsync(string message, int timeoutInSeconds = 10)
    {
        if (_channel == null)
        {
            throw new InvalidOperationException("Channel is not initialized.");
        }

        var body = Encoding.UTF8.GetBytes(message);
        var props = _channel.CreateBasicProperties();
        var correlationId = Guid.NewGuid().ToString();
        props.CorrelationId = correlationId;
        props.ReplyTo = _responseQueue;

        var tcs = new TaskCompletionSource<string>();

        // Збереження TaskCompletionSource для очікування відповіді з правильним correlationId
        _pendingResponses[correlationId] = tcs;

        // Публікація повідомлення в чергу
        _channel.BasicPublish(exchange: "", routingKey: _requestQueue, basicProperties: props, body: body);
        Console.WriteLine($"[x] Sent: {message}");

        try
        {
            // Очікування відповіді з таймаутом
            return await tcs.Task.WaitAsync(TimeSpan.FromSeconds(timeoutInSeconds));
        }
        catch (TimeoutException)
        {
            Console.WriteLine($"[x] Error: The response was not received within the expected time frame. Timeout: {timeoutInSeconds}");
            _pendingResponses.TryRemove(correlationId, out _); // Видалення TaskCompletionSource у випадку таймауту
            throw;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[x] Error while waiting for response: {ex.Message}");
            _pendingResponses.TryRemove(correlationId, out _); // Видалення TaskCompletionSource у випадку помилки
            throw;
        }
    }

    public void Close()
    {
        try
        {
            _channel?.Close();
            _connection?.Close();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[x] Error while closing RabbitMQ connection: {ex.Message}");
        }
    }
}
