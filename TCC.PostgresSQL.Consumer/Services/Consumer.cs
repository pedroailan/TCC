using Npgsql;
using System.Text.Json;
using TCC.Commons;

namespace TCC.PostgreSQL.Consumer.Services;

public class Consumer(IConfiguration configuration, Response response, ILogger<Consumer> logger) : BackgroundService
{
    private readonly IConfiguration _configuration = configuration;
    private readonly Response _response = response;
    private readonly ILogger<Consumer> _logger = logger;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            using var conn = new NpgsqlConnection(_configuration.GetConnectionString("Database"));
            conn.Open();

            if (conn.State == System.Data.ConnectionState.Open)
            {
                _logger.LogWarning("{Message}", "Conexão aberta com sucesso!");
            }


            using (var listenCommand = conn.CreateCommand())
            {
                listenCommand.CommandText = "LISTEN Channel01";
                listenCommand.ExecuteNonQuery();
            }

            conn.Notification += async (o, e) =>
            {
                Notification message = JsonSerializer.Deserialize<Notification>(e.Payload);

                long receivedTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

                long latency = message.CalculateTime(receivedTimestamp);

                await _response.Notification(message);
                _logger.LogInformation("{Message};{Line}", DateTime.Now.ToString("HH:mm:ss:fff"), latency);
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