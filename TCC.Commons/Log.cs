using Microsoft.Extensions.Logging;
using Serilog;

namespace TCC.Commons;

public static class Log
{
    public static ILoggingBuilder AddLogs(this ILoggingBuilder loggingBuilder)
    {
        loggingBuilder.ClearProviders();
        loggingBuilder.AddSerilog(
            new LoggerConfiguration()
            .WriteTo.File(
                restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information,
                path: "Metrics/output.csv",
                outputTemplate: "{Message}{NewLine}",
                rollingInterval: RollingInterval.Day
            )
            .WriteTo.File(
                restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Warning,
                path: $"Logs/{DateTime.Now:MM-yyyy}/.txt",
                rollingInterval: RollingInterval.Day
                )
            .CreateLogger());
        return loggingBuilder;
    }
}