using Microsoft.Extensions.Configuration;
using OpenTelemetry;
using OpenTelemetry.Logs;

namespace ivp.edm.apm;
internal static class LoggerExtensions
{
    public static OpenTelemetryLoggerOptions AddSerilogExporter(this OpenTelemetryLoggerOptions options, IConfiguration configuration)
    {
        if (options == null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        return options.AddProcessor(new BatchLogRecordExportProcessor(new SerilogExporter(options, configuration)));
    }
}