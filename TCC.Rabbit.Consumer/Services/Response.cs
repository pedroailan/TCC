using RabbitMQ.Client;
using System.Text.Json;
using System.Text;
using TCC.Commons;

namespace TCC.Rabbit.Consumer.Services;

public class Response
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly ILogger<Response> _logger;

    public Response(IConfiguration configuration, ILogger<Response> logger)
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

    public void Notification(Notification notification)
    {
        var messageBody = JsonSerializer.Serialize(notification);
        var body = Encoding.UTF8.GetBytes(messageBody);

        _channel.BasicPublish(exchange: "",
                             routingKey: "api_queue_response",
                             basicProperties: null,
                             body: body);
    }

    ~Response()
    {
        _channel?.Close();
        _connection?.Close();
    }
}
