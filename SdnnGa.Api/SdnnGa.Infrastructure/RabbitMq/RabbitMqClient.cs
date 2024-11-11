using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SdnnGa.Model.Infrastructure.Interfaces.RabbitMq;
using System;
using System.Text;
using System.Threading;
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

    public RabbitMqClient(string hostname, string requestQueue, string responseQueue)
    {
        _hostname = hostname;
        _requestQueue = requestQueue;
        _responseQueue = responseQueue;
        _factory = new ConnectionFactory() { HostName = _hostname }; // Згідно з документацією, можна додати й інші властивості, такі як UserName, Password тощо.
        InitializeConnection();
    }

    private void InitializeConnection()
    {
        _connection = _factory.CreateConnection();
        _channel = _connection.CreateModel();

        _channel.QueueDeclare(queue: _requestQueue, durable: false, exclusive: false, autoDelete: false, arguments: null);
        _channel.QueueDeclare(queue: _responseQueue, durable: false, exclusive: false, autoDelete: false, arguments: null);
    }

    public async Task<string> SendMessageAsync(string message)
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

        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += (model, ea) =>
        {
            if (ea.BasicProperties.CorrelationId == correlationId)
            {
                var responseBody = ea.Body.ToArray();
                var response = Encoding.UTF8.GetString(responseBody);
                Console.WriteLine($"[x] Received response: {response}");
                tcs.TrySetResult(response);
            }
        };

        // Споживач повинен бути зареєстрований на початку для коректної обробки повідомлень, згідно з рекомендаціями документації
        _channel.BasicConsume(queue: _responseQueue, autoAck: true, consumer: consumer);

        // Публікація повідомлення в чергу
        _channel.BasicPublish(exchange: "", routingKey: _requestQueue, basicProperties: props, body: body);
        Console.WriteLine($"[x] Sent: {message}");

        try
        {
            using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10)))
            {
                var completedTask = await Task.WhenAny(tcs.Task, Task.Delay(-1, cts.Token));
                if (completedTask == tcs.Task)
                {
                    return await tcs.Task;
                }
                else
                {
                    throw new TimeoutException("The response was not received within the expected time frame.");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[x] Error while waiting for response: {ex.Message}");
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