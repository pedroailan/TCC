using Npgsql;
using Prometheus;
using System.Diagnostics;
using System.Text.Json;
using TCC.Commons;

namespace TCC.PostgreSQL.Producer.Services;

public class ConsumerResponse(IConfiguration configuration, ILogger<ConsumerResponse> logger) : BackgroundService
{
    private readonly IConfiguration _configuration = configuration;
    private readonly ILogger<ConsumerResponse> _logger = logger;
    private static readonly Histogram _latencyHistogram = Metrics.CreateHistogram(
        "app_operation_latency_milliseconds", 
        "Latência da operação em segundos",
        new HistogramConfiguration
        {
            Buckets = Histogram.ExponentialBuckets(1, 2, 10)
        });

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            using var conn = new NpgsqlConnection(_configuration.GetConnectionString("Database"));
            conn.Open();

            using (var listenCommand = conn.CreateCommand())
            {
                listenCommand.CommandText = $"LISTEN Channel02";
                listenCommand.ExecuteNonQuery();
            }

            conn.Notification += (o, e) =>
            {
                Notification message = JsonSerializer.Deserialize<Notification>(e.Payload);

                long receivedTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

                long latency = message.CalculateTime(receivedTimestamp);

                _latencyHistogram.Observe(latency);
            };

            while (true)
            {
                await conn.WaitAsync(stoppingToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogCritical("{Message}", ex.ToString());
        }
    }
}
