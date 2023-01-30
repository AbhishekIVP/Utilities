using ivp.edm.validations;
using Microsoft.Extensions.Configuration;
using OpenTelemetry;
using OpenTelemetry.Logs;

namespace ivp.edm.apm;
internal static class LoggerExtensions
{
    public static OpenTelemetryLoggerOptions AddSerilogExporter(this OpenTelemetryLoggerOptions options, IConfiguration configuration)
    {
        ArgumentGuard.NotNull<OpenTelemetryLoggerOptions>(options);
        return options.AddProcessor(new BatchLogRecordExportProcessor(new SerilogExporter(options, configuration)));
    }
}