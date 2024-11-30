using Prometheus;
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
    private static readonly Histogram _latencyHistogram = Metrics.CreateHistogram(
        "app_operation_latency_milliseconds",
        "Latência da operação em segundos",
        new HistogramConfiguration
        {
            Buckets = Histogram.ExponentialBuckets(1, 2, 10)
        });

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
            Task.Run(() =>
            {
                var body = ea.Body.ToArray();
                var str = Encoding.UTF8.GetString(body);

                Notification message = JsonSerializer.Deserialize<Notification>(str);

                long receivedTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                long latency = message.CalculateTime(receivedTimestamp);

                _latencyHistogram.Observe(latency);
            });
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