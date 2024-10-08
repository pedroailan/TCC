using TCC.Commons;

namespace TCC.PostgreSQL.Producer.Services;

public class Job(Producer producer) : BackgroundService
{
    private readonly Producer _producer = producer;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            await _producer.Notification(new Notification());
            await Task.Delay(Config.Interval, stoppingToken);
        }
    }
}