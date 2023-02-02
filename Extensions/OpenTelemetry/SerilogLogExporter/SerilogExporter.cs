using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Logs;
using Serilog;
using Serilog.Events;

namespace ivp.edm.apm;

class SerilogExporter : BaseExporter<LogRecord>
{
    private readonly string name;
    private readonly OpenTelemetryLoggerOptions options;
    private readonly IConfiguration configuration;
    private Dictionary<string, string>? resources;

    private readonly Dictionary<string, string> _staticEnrichers;
    public SerilogExporter(OpenTelemetryLoggerOptions options,
                            IConfiguration configuration,
                            string name = "SerilogExporter")
    {
        this.name = name;
        this.options = options;
        this.configuration = configuration;
        Log.Logger = new LoggerConfiguration()
            .WriteTo.File(path: configuration["OpenTelemetry:Logging:OtelSerilog:Path"] ?? "log/service.log",
                rollingInterval: RollingInterval.Day,
                rollOnFileSizeLimit: true,
                fileSizeLimitBytes: 100000,
                outputTemplate: "{Message:j}{NewLine}")
            .CreateLogger();
        _staticEnrichers = StaticEnrichers();
    }
    public override ExportResult Export(in Batch<LogRecord> batch)
    {
        using var scope = SuppressInstrumentationScope.Begin();
        try
        {
            resources = this.ParentProvider?.GetResource()?.Attributes.ToDictionary(_ => _.Key, _ => $"{_.Value}");
            foreach (var record in batch)
            {
                var logRecord = new SerilogLogRecord();
                logRecord.Timestamp = record.Timestamp;
                if (record.TraceId != default)
                {
                    logRecord.spanid = record.SpanId.ToString();
                    logRecord.traceid = record.TraceId.ToString();
                    logRecord.traceflags = record.TraceFlags.ToString();
                }
                if (record.CategoryName != null)
                {
                    logRecord.category.Add(nameof(record.CategoryName), record.CategoryName);
                    logRecord.category.Add("dotnet.ilogger.category", record.CategoryName);
                }
                logRecord.severity = record.LogLevel.ToString();
                logRecord.body = $"{record.FormattedMessage}";

                if (record.State != null)
                {
                    if (record.State is IReadOnlyList<KeyValuePair<string, object>> recordStates)
                    {
                        for (int i = 0; i < recordStates.Count; i++)
                        {
                            logRecord.stateValues.Add(recordStates[i].Key, $"{recordStates[i].Value}");
                        }
                    }
                    else
                    {
                        logRecord.stateValues.Add(nameof(record.State), $"{record.State}");
                    }
                }
                else if (record.StateValues != null)
                {
                    for (int i = 0; i < record.StateValues.Count; i++)
                    {
                        logRecord.stateValues.Add(record.StateValues[i].Key, $"{record.StateValues[i].Value}");
                    }
                }

                if (record.EventId != default)
                {
                    logRecord.eventValues.Add(nameof(record.EventId.Id), $"{record.EventId.Id}");
                    if (string.IsNullOrEmpty(record.EventId.Name) == false)
                    {
                        logRecord.eventValues.Add(nameof(record.EventId.Name), $"{record.EventId.Name}");
                    }
                }

                if (record.Exception != null)
                {
                    logRecord.exception = $"{record.Exception}";
                }

                record.ForEachScope((scope, _logRecord) =>
                {
                    foreach (var scopeItem in scope)
                    {
                        _logRecord.scopeValues.Add(scopeItem.Key, $"{scopeItem.Value}");
                    }
                }, logRecord);

                logRecord.resources = resources;

                AddEnrichers(logRecord);

                LogEventLevel? logEventLevel = GetSerilogLogEventLevel(record.LogLevel);
                if (logEventLevel != null)
                    Log.Write<SerilogLogRecord>(logEventLevel.Value, "{@SerilogLogRecord}", logRecord);
            }
        }
        catch (Exception ex)
        {
            Log.Fatal($"{this.name} has Failed. {ex}");
            return ExportResult.Failure;
        }
        return ExportResult.Success;
    }
    private LogEventLevel? GetSerilogLogEventLevel(LogLevel logLevel)
    {
        LogEventLevel? logEventLevel;
        switch (logLevel)
        {
            case LogLevel.Information:
                logEventLevel = LogEventLevel.Information;
                break;
            case LogLevel.Critical:
                logEventLevel = LogEventLevel.Fatal;
                break;
            case LogLevel.Trace:
                logEventLevel = LogEventLevel.Verbose;
                break;
            case LogLevel.Debug:
                logEventLevel = LogEventLevel.Debug;
                break;
            case LogLevel.Error:
                logEventLevel = LogEventLevel.Error;
                break;
            case LogLevel.Warning:
                logEventLevel = LogEventLevel.Warning;
                break;
            case LogLevel.None:
            default:
                logEventLevel = null;
                break;
        }
        return logEventLevel;
    }
    protected override bool OnShutdown(int timeoutMilliseconds)
    {
        var logRecord = new SerilogLogRecord()
        {
            Timestamp = DateTime.Now,
            body = $"{this.name}.OnShutdown(timeoutMilliseconds={timeoutMilliseconds})",
            severity = "Information"
        };
        Log.Information("{@SerilogLogRecord}", logRecord);
        return true;
    }
    protected override void Dispose(bool disposing)
    {
        var logRecord = new SerilogLogRecord()
        {
            Timestamp = DateTime.Now,
            body = $"{this.name}.Dispose({disposing})",
            severity = "Information"
        };
        Log.Information("{@SerilogLogRecord}", logRecord);
    }
    private Dictionary<string, string> StaticEnrichers()
    {
        Dictionary<string, string> _enrichers = new Dictionary<string, string>();
        _enrichers.Add("EnvironmentName", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production");
        _enrichers.Add("MachineName", Environment.MachineName);
        _enrichers.Add("ProcessId", $"{GetProcessId()}");

        return _enrichers;
    }
    private int GetProcessId()
    {
#if FEATURE_ENVIRONMENT_PID
        return System.Environment.ProcessId;
#else
        using var process = System.Diagnostics.Process.GetCurrentProcess();
        return process.Id;
#endif
    }
    private void AddEnrichers(SerilogLogRecord logRecord)
    {
        logRecord.enrichers.Add("ThreadId", $"{Environment.CurrentManagedThreadId}");
        foreach (var _staticEnricher in _staticEnrichers)
        {
            logRecord.enrichers.Add(_staticEnricher.Key, _staticEnricher.Value);
        }
    }

    public class SerilogLogRecord
    {
        public SerilogLogRecord()
        {
            body = string.Empty;
            traceid = string.Empty;
            traceflags = string.Empty;
            spanid = string.Empty;
            severity = string.Empty;
            exception = string.Empty;
            stateValues = new Dictionary<string, string?>();
            category = new Dictionary<string, string?>();
            eventValues = new Dictionary<string, string?>();
            scopeValues = new Dictionary<string, string?>();
            enrichers = new Dictionary<string, string?>();
        }
        public DateTime Timestamp { get; set; }
        public string body { get; set; }
        public string traceid { get; set; }
        public string traceflags { get; set; }
        public string spanid { get; set; }
        public string severity { get; set; }
        public string exception { get; set; }
        public Dictionary<string, string?> eventValues { get; set; }
        public Dictionary<string, string?> category { get; set; }
        public Dictionary<string, string?> stateValues { get; set; }
        public Dictionary<string, string?> scopeValues { get; set; }
        public Dictionary<string, string?> enrichers { get; set; }
        public Dictionary<string, string>? resources { get; set; }

    }
}