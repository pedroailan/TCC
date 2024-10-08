using Npgsql;
using System.Text.Json;
using TCC.Commons;

namespace TCC.PostgreSQL.Consumer.Services;

public class Response(IConfiguration configuration, ILogger<Response> logger)
{
    private readonly IConfiguration _configuration = configuration;
    private readonly ILogger<Response> _logger = logger;

    public async Task Notification(Notification notification)
    {
        try
        {
            using var conn = new NpgsqlConnection(_configuration.GetConnectionString("Database"));
            conn.Open();

            using var cmd = new NpgsqlCommand($"NOTIFY Channel02, '{JsonSerializer.Serialize(notification)}'", conn);

            await cmd.ExecuteNonQueryAsync();
        }
        catch (Exception ex)
        {
            _logger.LogCritical("{Message}", ex.ToString());
        }
    }
}