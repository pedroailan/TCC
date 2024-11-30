using Microsoft.AspNetCore.Connections;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using TCC.Commons;

namespace TCC.Rabbit.Consumer.Services;

public class Consumer
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly Response _response;
    private readonly ILogger<Consumer> _logger;

    public Consumer(IConfiguration configuration, Response response, ILogger<Consumer> logger)
    {
        _logger = logger;

        ConnectionFactory factory = Builder.Build(configuration);
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        _channel.QueueDeclare(queue: "api_queue",
                             durable: false,
                             exclusive: false,
                             autoDelete: false,
                             arguments: null);
        _response = response;
    }

    public void StartConsuming()
    {
        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += (model, ea) =>
        {
            Task.Run(() =>
            {
                var body = ea.Body.ToArray();
                var str = Encoding.UTF8.GetString(body);

                Notification message = JsonSerializer.Deserialize<Notification>(str);

                long receivedTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                long latency = message.CalculateTime(receivedTimestamp);

                _response.Notification(message);
            });
        };
        _channel.BasicConsume(queue: "api_queue",
                             autoAck: true,
                             consumer: consumer);
    }

    ~Consumer()
    {
        _channel?.Close();
        _connection?.Close();
    }
}
