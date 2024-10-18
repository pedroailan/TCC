using Prometheus;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using TCC.Commons;

namespace TCC.Rabbit.Producer.Services;

public class Producer : IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private static readonly Counter _messagesRequestsCounter = Metrics.CreateCounter("messages_request_total", "Total number of successful requests.");

    public Producer(IConfiguration configuration)
    {
        ConnectionFactory factory = Builder.Build(configuration);
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        _channel.QueueDeclare(queue: "api_queue",
                             durable: false,
                             exclusive: false,
                             autoDelete: false,
                             arguments: null);
    }

    public async Task Notification(Notification notification)
    {
        var messageBody = JsonSerializer.Serialize(notification);
        var body = Encoding.UTF8.GetBytes(messageBody);

        await Task.Run(() =>
            _channel.BasicPublish(exchange: "",
                                 routingKey: "api_queue",
                                 basicProperties: null,
                                 body: body)
        );
        _messagesRequestsCounter.Inc();
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    ~Producer()
    {
        _channel?.Close();
        _connection?.Close();
    }
}
