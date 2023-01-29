using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using OpenTelemetry.Logs;
using Serilog;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Reflection;
using OpenTelemetry.Resources;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Builder;
using StackExchange.Redis;

namespace ivp.edm.apm
{
    public static class MonitoringServiceCollectionExtensions
    {
        public static IHostBuilder AddMonitoring(this IHostBuilder builder, IConnectionMultiplexer? redisConnection = null)
        {
            return builder.ConfigureServices((context, collection) =>
            {
                if (redisConnection == null)
                {
                    using (var _serviceProvider = collection.BuildServiceProvider())
                        redisConnection = _serviceProvider.GetService<IConnectionMultiplexer>();
                }
                collection.AddMonitoring(context.Configuration, context.HostingEnvironment, redisConnection);
            });
        }

        public static IServiceCollection AddMonitoring(this IServiceCollection services
            , IConfiguration configuration
            , IHostEnvironment environment
            , IConnectionMultiplexer? redisConnection = null
            )
        {
            var serviceName = configuration["OpenTelemetry:Service:Name"]?.ToString() ?? environment.ApplicationName;
            var serviceVersion = configuration["OpenTelemetry:Service:Version"]?.ToString() ?? Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "unknown";
            var otlpEndpoint = configuration["OpenTelemetry:Endpoint"]?.ToString() ?? (environment.IsProduction() ? "http://otel-collector:4317" : "http://localhost:4317");

            var appResourceBuilder = ResourceBuilder.CreateDefault()
                            .AddService(
                                serviceName: serviceName,
                                serviceVersion: serviceVersion);

            // Use IConfiguration binding for AspNetCore instrumentation options.
            // services.Configure<AspNetCoreInstrumentationOptions>(builder.Configuration.GetSection("AspNetCoreInstrumentation"));

            // Add services to the container.
            OpenTelemetryBuilder _openTelemetryBuilder = services
                                                            .AddSingleton(new Observability
                                                            {
                                                                ServiceActivity = new ActivitySource(serviceName),
                                                                ServiceMeter = new Meter(serviceName)
                                                            })
                                                            .AddOpenTelemetry();
            MetricsMode _metricsMode;
            if (Enum.TryParse<MetricsMode>(configuration["OpenTelemetry:Metrics:Mode"], out _metricsMode) == false)
                _metricsMode = MetricsMode.Otel;

            TracesMode _tracesMode;
            if (Enum.TryParse<TracesMode>(configuration["OpenTelemetry:Traces:Mode"], out _tracesMode) == false)
                _tracesMode = TracesMode.Otel;

            if (_tracesMode == TracesMode.Otel)
            {
                _openTelemetryBuilder.WithTracing(tracerProviderBuilder =>
                        {
                            tracerProviderBuilder
                                .AddSource(serviceName)
                                .SetResourceBuilder(appResourceBuilder)
                                .AddHttpClientInstrumentation(_ =>
                                {
                                    //TODO: Use Options Pattern
                                    _.RecordException = Convert.ToBoolean(configuration["OpenTelemetry:Traces:Http:RecordException"]);
                                })
                                .AddAspNetCoreInstrumentation(_ =>
                                {
                                    //TODO: Use Options Pattern
                                    _.RecordException = Convert.ToBoolean(configuration["OpenTelemetry:Traces:AspNetCore:RecordException"]);
                                    _.EnableGrpcAspNetCoreSupport = Convert.ToBoolean(configuration["OpenTelemetry:Traces:AspNetCore:EnableGrpcAspNetCoreSupport"]);
                                })
                                .AddSqlClientInstrumentation(_ =>
                                {
                                    //TODO: Use Options Pattern
                                    _.RecordException = Convert.ToBoolean(configuration["OpenTelemetry:Traces:Db:RecordException"]);
                                    _.SetDbStatementForStoredProcedure = Convert.ToBoolean(configuration["OpenTelemetry:Traces:Db:CaptureStoreProcCall"]);
                                    _.SetDbStatementForText = Convert.ToBoolean(configuration["OpenTelemetry:Traces:Db:CaptureQueryText"]);
                                })
                                .AddMySqlDataInstrumentation(_ =>
                                {
                                    //TODO: Use Options Pattern
                                    _.RecordException = Convert.ToBoolean(configuration["OpenTelemetry:Traces:Db:RecordException"]);
                                    _.SetDbStatement = Convert.ToBoolean(configuration["OpenTelemetry:Traces:Db:CaptureQueryText"]);
                                })
                                .AddEntityFrameworkCoreInstrumentation(_ =>
                                {
                                    //TODO: Use Options Pattern
                                    _.SetDbStatementForStoredProcedure = Convert.ToBoolean(configuration["OpenTelemetry:Traces:Db:CaptureStoreProcCall"]);
                                    _.SetDbStatementForText = Convert.ToBoolean(configuration["OpenTelemetry:Traces:Db:CaptureQueryText"]);
                                })
                                .AddOtlpExporter(opts =>
                                {
                                    opts.Endpoint = new Uri(otlpEndpoint);
                                })
                                ;
                            if (redisConnection != null)
                                tracerProviderBuilder.AddRedisInstrumentation(redisConnection);
                            if (environment.IsProduction())
                                tracerProviderBuilder.SetSampler<TraceIdRatioBasedSampler>();
                            else
                                tracerProviderBuilder.SetSampler<AlwaysOnSampler>();
                        });
            }

            if (_metricsMode != MetricsMode.None)
            {
                _openTelemetryBuilder.WithMetrics(metricProviderBuilder =>
                        {
                            metricProviderBuilder
                                .AddMeter(serviceName)
                                .SetResourceBuilder(appResourceBuilder)
                                .AddAspNetCoreInstrumentation()
                                .AddHttpClientInstrumentation()
                                .AddRuntimeInstrumentation()
                                .AddProcessInstrumentation()
                                // .AddEventCountersInstrumentation(eventCounterConfigure =>
                                // {
                                //     eventCounterConfigure.RefreshIntervalSecs = 1;
                                //     eventCounterConfigure.AddEventSources(
                                //         "Microsoft.AspNetCoreHosting",
                                //         "System.Net.Http",
                                //         "System.Net.Sockets",
                                //         "System.Net.NameResolution",
                                //         "System.Net.Security"
                                //     );
                                // })
                                ;

                            if (_metricsMode == MetricsMode.Otel)
                                metricProviderBuilder.AddOtlpExporter(opts =>
                                    {
                                        opts.Endpoint = new Uri(otlpEndpoint);
                                    })
                                    ;
                            else if (_metricsMode == MetricsMode.Prometheus)
                                metricProviderBuilder.AddPrometheusExporter()
                                    ;
                            else if (_metricsMode == MetricsMode.Console)
                                metricProviderBuilder.AddConsoleExporter()
                                    ;
                        });
            }

            _openTelemetryBuilder.StartWithHost();
            return services;
        }
    }

    public static class LoggingServiceCollectionExtensions
    {
        public static IHostBuilder AddLogging(this IHostBuilder builder, ResourceBuilder? resourceBuilder = null)
        {
            return builder.ConfigureLogging((context, loggingBuilder) =>
            {
                loggingBuilder.AddLogging(context.Configuration, context.HostingEnvironment, resourceBuilder);
            });
        }

        public static ILoggingBuilder AddLogging(this ILoggingBuilder builder, IConfiguration configuration, IHostEnvironment environment, ResourceBuilder? resourceBuilder)
        {
            var serviceName = configuration["OpenTelemetry:Service:Name"]?.ToString() ?? environment.ApplicationName;
            var serviceVersion = configuration["OpenTelemetry:Service:Version"]?.ToString() ?? Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "unknown";
            var otlpEndpoint = configuration["OpenTelemetry:Endpoint"]?.ToString() ?? (environment.IsProduction() ? "http://otel-collector:4317" : "http://localhost:4317");

            var appResourceBuilder = resourceBuilder ?? ResourceBuilder.CreateDefault()
                            .AddService(
                                serviceName: serviceName,
                                serviceVersion: serviceVersion);

            builder.ClearProviders();

            LoggingMode _loggingMode;
            if (Enum.TryParse<LoggingMode>(configuration["OpenTelemetry:Logging:Mode"], out _loggingMode) == false)
                _loggingMode = LoggingMode.Otel;

            switch (_loggingMode)
            {
                case LoggingMode.OtelSerilog:
                    builder.AddOpenTelemetry(logProviderBuilder =>
                            {
                                logProviderBuilder.SetResourceBuilder(appResourceBuilder)
                                .AddSerilogExporter(configuration)
                                ;
                            })
                            ;
                    break;
                case LoggingMode.Otel:
                default:
                    builder.AddOpenTelemetry(logProviderBuilder =>
                            {
                                logProviderBuilder.SetResourceBuilder(appResourceBuilder)
                                .AddOtlpExporter(opts =>
                                {
                                    opts.Endpoint = new Uri(otlpEndpoint);
                                })
                                ;
                            })
                            ;
                    break;
                case LoggingMode.Serilog:
                    builder.AddSerilog(new LoggerConfiguration()
                        .ReadFrom.Configuration(configuration)
                        .Enrich.WithProperty("ServiceName", serviceName)
                        .Enrich.WithProperty("ServiceVersion", serviceVersion)
                        .CreateLogger()
                );
                    break;
                case LoggingMode.Console:
                    builder.AddOpenTelemetry(logProviderBuilder =>
                            {
                                logProviderBuilder.SetResourceBuilder(appResourceBuilder)
                                .AddConsoleExporter()
                                ;
                            })
                            ;
                    break;
                case LoggingMode.None:
                    break;
            }
            return builder;
        }
    }

    public static class WebApplicationExtensions
    {
        public static void UseOpenTelemetry(this WebApplication applicationBuilder)
        {
            MetricsMode _metricsMode;
            Enum.TryParse<MetricsMode>(applicationBuilder.Configuration["OpenTelemetry:Metrics:Mode"], out _metricsMode);
            if (_metricsMode == MetricsMode.Prometheus)
                applicationBuilder.UseOpenTelemetryPrometheusScrapingEndpoint();
        }
    }

    public enum LoggingMode
    {
        Otel,
        OtelSerilog,
        Serilog,
        Console,
        None
    }

    public enum MetricsMode
    {
        Otel,
        None,
        Prometheus,
        Console
    }

    public enum TracesMode
    {
        Otel,
        None
    }
}

