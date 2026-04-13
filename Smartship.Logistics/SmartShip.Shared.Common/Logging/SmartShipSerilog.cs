using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;

namespace SmartShip.Shared.Common.Logging;

/// <summary>
/// Centralized Serilog configuration for SmartShip services.
/// </summary>
public static class SmartShipSerilog
{
    private const string PersistLogsConfigKey = "SmartShipLogging:PersistLogs";

    /// <summary>
    /// Configures Serilog consistently across services and supports disabling persistent sinks (File/Seq).
    /// </summary>
    /// <param name="configuration">Application configuration.</param>
    /// <param name="loggerConfiguration">Serilog logger configuration builder.</param>
    /// <param name="applicationName">Logical service name (e.g., SmartShip.IdentityService).</param>
    public static void Configure(IConfiguration configuration, LoggerConfiguration loggerConfiguration, string applicationName)
    {
        var persistLogs = configuration.GetValue(PersistLogsConfigKey, defaultValue: true);

        // Minimum level + overrides (production-friendly defaults)
        var minimumDefault = ParseLevel(configuration["Serilog:MinimumLevel:Default"], LogEventLevel.Information);
        loggerConfiguration.MinimumLevel.Is(minimumDefault);

        var overrideSection = configuration.GetSection("Serilog:MinimumLevel:Override");
        foreach (var child in overrideSection.GetChildren())
        {
            if (string.IsNullOrWhiteSpace(child.Key) || string.IsNullOrWhiteSpace(child.Value))
            {
                continue;
            }

            loggerConfiguration.MinimumLevel.Override(child.Key, ParseLevel(child.Value, LogEventLevel.Warning));
        }

        // Enrichment
        loggerConfiguration
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithThreadId()
            .Enrich.WithEnvironmentName()
            .Enrich.WithProperty("ApplicationName", applicationName);

        // Always keep console logging for runtime visibility (containers / dev / prod).
        loggerConfiguration.WriteTo.Console();

        if (!persistLogs)
        {
            // PersistLogs=false means: don't write logs to storage (file/seq/etc).
            return;
        }

        // Reuse existing Serilog:WriteTo configuration for sink arguments when present.
        var writeTo = configuration.GetSection("Serilog:WriteTo").GetChildren().ToArray();

        var fileSink = writeTo.FirstOrDefault(x => string.Equals(x["Name"], "File", StringComparison.OrdinalIgnoreCase));
        if (fileSink is not null)
        {
            var path = fileSink["Args:path"] ?? $"Logs/{applicationName}-.txt";
            var retained = configuration.GetValue<int?>("Serilog:WriteTo:1:Args:retainedFileCountLimit") ?? 14;
            var sizeLimit = configuration.GetValue<long?>("Serilog:WriteTo:1:Args:fileSizeLimitBytes") ?? 10 * 1024 * 1024;
            var shared = configuration.GetValue<bool?>("Serilog:WriteTo:1:Args:shared") ?? true;
            var rollOnSize = configuration.GetValue<bool?>("Serilog:WriteTo:1:Args:rollOnFileSizeLimit") ?? true;
            var template = fileSink["Args:outputTemplate"]
                ?? "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}";

            var rollingInterval = ParseRollingInterval(fileSink["Args:rollingInterval"]);

            loggerConfiguration.WriteTo.File(
                path: path,
                rollingInterval: rollingInterval,
                fileSizeLimitBytes: sizeLimit,
                rollOnFileSizeLimit: rollOnSize,
                retainedFileCountLimit: retained,
                shared: shared,
                outputTemplate: template);
        }

        var seqSink = writeTo.FirstOrDefault(x => string.Equals(x["Name"], "Seq", StringComparison.OrdinalIgnoreCase));
        if (seqSink is not null)
        {
            var serverUrl = seqSink["Args:serverUrl"];
            if (!string.IsNullOrWhiteSpace(serverUrl))
            {
                loggerConfiguration.WriteTo.Seq(serverUrl);
            }
        }
    }

    private static LogEventLevel ParseLevel(string? value, LogEventLevel fallback)
        => Enum.TryParse<LogEventLevel>(value, ignoreCase: true, out var parsed) ? parsed : fallback;

    private static RollingInterval ParseRollingInterval(string? value)
        => Enum.TryParse<RollingInterval>(value, ignoreCase: true, out var parsed) ? parsed : RollingInterval.Day;
}

