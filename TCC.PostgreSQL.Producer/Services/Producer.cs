using Npgsql;
using Prometheus;
using System.Text.Json;
using TCC.Commons;

namespace TCC.PostgreSQL.Producer.Services;

public class Producer(IConfiguration configuration, ILogger<Producer> logger)
{
    private readonly IConfiguration _configuration = configuration;
    private readonly ILogger<Producer> _logger = logger;
    private static readonly Counter _messagesRequestsCounter = Metrics.CreateCounter("messages_request_total", "Total number of successful requests.");

    public async Task Notification(Notification notification)
    {
        try
        {
            using var conn = new NpgsqlConnection(_configuration.GetConnectionString("Database"));
            conn.Open();

            if (conn.State != System.Data.ConnectionState.Open)
            {
                _logger.LogCritical("{Message}", "Erro ao abrir conexão!");
            }

            using var cmd = new NpgsqlCommand($"NOTIFY Channel01, '{JsonSerializer.Serialize(notification)}'", conn);

            await cmd.ExecuteNonQueryAsync();
            _messagesRequestsCounter.Inc();
        }
        catch (Exception ex)
        {
            _logger.LogCritical("{Message}", ex.ToString());
        }
    }
}