using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using TCC.Commons;

namespace TCC.Rabbit.Producer.Services;

public class ConsumerResponse
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly ILogger<ConsumerResponse> _logger;

    public ConsumerResponse(IConfiguration configuration, ILogger<ConsumerResponse> logger)
    {
        _logger = logger;

        ConnectionFactory factory = Builder.Build(configuration);
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        _channel.QueueDeclare(queue: "api_queue_response",
                             durable: false,
                             exclusive: false,
                             autoDelete: false,
                             arguments: null);

    }

    public void StartConsuming()
    {
        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var str = Encoding.UTF8.GetString(body);

            Notification message = JsonSerializer.Deserialize<Notification>(str);

            long receivedTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            long latency = message.CalculateTime(receivedTimestamp);


            _logger.LogInformation("{Message};{Line}", DateTime.Now.ToString("HH:mm:ss:fff"), latency);
        };
        _channel.BasicConsume(queue: "api_queue_response",
                             autoAck: true,
                             consumer: consumer);
    }

    ~ConsumerResponse()
    {
        _channel?.Close();
        _connection?.Close();
    }
}