using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Reflection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Builder;
using StackExchange.Redis;
using OpenTelemetry.Instrumentation.AspNetCore;
using OpenTelemetry.Instrumentation.Http;
using OpenTelemetry.Instrumentation.MySqlData;
using OpenTelemetry.Instrumentation.SqlClient;
using OpenTelemetry.Instrumentation.EntityFrameworkCore;
using OpenTelemetry.Instrumentation.GrpcNetClient;

namespace ivp.edm.apm
{
    public static class MonitoringServiceCollectionExtensions
    {
        public static IHostBuilder AddMonitoring(this IHostBuilder builder, Action<IConnectionMultiplexer?>? setRedisConnection = null)
        {
            return builder.ConfigureServices((context, collection) =>
            {
                collection.AddMonitoring(context.Configuration, context.HostingEnvironment, setRedisConnection);
            });
        }

        public static IServiceCollection AddMonitoring(this IServiceCollection services
            , IConfiguration configuration
            , IHostEnvironment environment
            , Action<IConnectionMultiplexer?>? setRedisConnection = null
            , Action<ObservabilityOptions>? setObservabilityOptions = null
            )
        {
            IConnectionMultiplexer? _redisConnection;
            using (var _serviceProvider = services.BuildServiceProvider())
                _redisConnection = _serviceProvider.GetService<IConnectionMultiplexer>();

            ObservabilityOptions _observabilityOptions = new ObservabilityOptions();
            configuration.GetSection("OpenTelemetry").Bind(_observabilityOptions);

            setRedisConnection?.Invoke(_redisConnection);
            setObservabilityOptions?.Invoke(_observabilityOptions);

            _observabilityOptions.Service.Name = _observabilityOptions.Service.Name ?? environment.ApplicationName;
            _observabilityOptions.Endpoint = _observabilityOptions.Endpoint ?? (environment.IsProduction() ? "http://otel-collector:4317" : "http://localhost:4317");

            var appResourceBuilder = ResourceBuilder.CreateDefault()
                            .AddService(
                                serviceName: _observabilityOptions.Service.Name,
                                serviceVersion: _observabilityOptions.Service.Version);

            OpenTelemetryBuilder _openTelemetryBuilder = services
                                                            .AddSingleton(new Observability
                                                            {
                                                                ServiceActivity = new ActivitySource(_observabilityOptions.Service.Name),
                                                                ServiceMeter = new Meter(_observabilityOptions.Service.Name)
                                                            })
                                                            .AddOpenTelemetry();
            if (_observabilityOptions.Traces.Mode == TracesMode.Otel)
            {
                _openTelemetryBuilder.WithTracing(tracerProviderBuilder =>
                        {
                            tracerProviderBuilder
                                .AddSource(_observabilityOptions.Service.Name)
                                .SetResourceBuilder(appResourceBuilder)
                                //These are given in Core
                                .AddHttpClientInstrumentation(_ => _ = _observabilityOptions.Traces.Http)
                                .AddAspNetCoreInstrumentation(_ => _ = _observabilityOptions.Traces.AspNetCore)
                                .AddSqlClientInstrumentation(_ => _ = _observabilityOptions.Traces.Sql)
                                .AddGrpcClientInstrumentation(_ => _ = _observabilityOptions.Traces.Grpc)
                                //These are in Contrib
                                .AddMySqlDataInstrumentation(_ => _ = _observabilityOptions.Traces.MySql)
                                .AddEntityFrameworkCoreInstrumentation(_ => _ = _observabilityOptions.Traces.EFCore)
                                .AddOtlpExporter(opts =>
                                {
                                    opts.Endpoint = new Uri(_observabilityOptions.Endpoint);
                                })
                                ;
                            if (_redisConnection != null)
                                tracerProviderBuilder.AddRedisInstrumentation(_redisConnection);
                            if (environment.IsProduction())
                                tracerProviderBuilder.SetSampler<TraceIdRatioBasedSampler>();
                            else
                                tracerProviderBuilder.SetSampler<AlwaysOnSampler>();
                        });
            }

            if (_observabilityOptions.Metrics.Mode != MetricsMode.None)
            {
                _openTelemetryBuilder.WithMetrics(metricProviderBuilder =>
                        {
                            metricProviderBuilder
                                .AddMeter(_observabilityOptions.Service.Name)
                                .SetResourceBuilder(appResourceBuilder)
                                .AddAspNetCoreInstrumentation()
                                .AddHttpClientInstrumentation()
                                .AddRuntimeInstrumentation()
                                .AddProcessInstrumentation()
                                ;

                            switch (_observabilityOptions.Metrics.Mode)
                            {
                                case MetricsMode.Otel:
                                    metricProviderBuilder.AddOtlpExporter(opts =>
                                        {
                                            opts.Endpoint = new Uri(_observabilityOptions.Endpoint);
                                        })
                                        ;
                                    break;
                                case MetricsMode.Prometheus:
                                    metricProviderBuilder.AddPrometheusExporter();
                                    break;
                                case MetricsMode.Console:
                                    metricProviderBuilder.AddConsoleExporter();
                                    break;
                            }
                        });
            }
            _openTelemetryBuilder.StartWithHost();
            return services;
        }
    }

    public static class LoggingServiceCollectionExtensions
    {
        public static IHostBuilder AddLogging(this IHostBuilder builder, ResourceBuilder? resourceBuilder = null, Action<ObservabilityOptions>? setObservabilityOptions = null)
        {
            return builder.ConfigureLogging((context, loggingBuilder) =>
            {
                loggingBuilder.AddLogging(context.Configuration, context.HostingEnvironment, resourceBuilder, setObservabilityOptions);
            });
        }

        public static ILoggingBuilder AddLogging(this ILoggingBuilder builder
                , IConfiguration configuration
                , IHostEnvironment environment
                , ResourceBuilder? resourceBuilder
                , Action<ObservabilityOptions>? setObservabilityOptions = null)
        {
            ObservabilityOptions _observabilityOptions = new ObservabilityOptions();
            configuration.GetSection("OpenTelemetry").Bind(_observabilityOptions);

            setObservabilityOptions?.Invoke(_observabilityOptions);

            _observabilityOptions.Service.Name = _observabilityOptions.Service.Name ?? environment.ApplicationName;
            _observabilityOptions.Endpoint = _observabilityOptions.Endpoint ?? (environment.IsProduction() ? "http://otel-collector:4317" : "http://localhost:4317");

            var appResourceBuilder = resourceBuilder ?? ResourceBuilder.CreateDefault()
                            .AddService(
                                serviceName: _observabilityOptions.Service.Name,
                                serviceVersion: _observabilityOptions.Service.Version);

            builder.ClearProviders();
            Console.WriteLine(_observabilityOptions.Logging.Mode);
            switch (_observabilityOptions.Logging.Mode)
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
                                    opts.Endpoint = new Uri(_observabilityOptions.Endpoint);
                                })
                                ;
                            })
                            ;
                    break;
                case LoggingMode.Serilog:
                    builder.AddSerilog(new LoggerConfiguration()
                        .ReadFrom.Configuration(configuration)
                        .Enrich.WithProperty("ServiceName", _observabilityOptions.Service.Name)
                        .Enrich.WithProperty("ServiceVersion", _observabilityOptions.Service.Version)
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
            ObservabilityOptions? _observabilityOptions = applicationBuilder.Services.GetService<ObservabilityOptions>();
            if (_observabilityOptions?.Metrics.Mode == MetricsMode.Prometheus)
                applicationBuilder.UseOpenTelemetryPrometheusScrapingEndpoint();
        }
    }

    public class ObservabilityOptions
    {
        internal ObservabilityOptions()
        {
            Logging = new LoggingOptions();
            Service = new ServiceOptions();
            Metrics = new MetricsOptions();
            Traces = new TracesOptions();
        }
        public string? Endpoint { get; set; }
        public LoggingOptions Logging { get; set; }
        public ServiceOptions Service { get; set; }
        public MetricsOptions Metrics { get; set; }
        public TracesOptions Traces { get; set; }
    }

    public class TracesOptions
    {
        public TracesOptions()
        {
            Sql = new SqlClientInstrumentationOptions();
            MySql = new MySqlDataInstrumentationOptions();
            EFCore = new EntityFrameworkInstrumentationOptions();
            Http = new HttpClientInstrumentationOptions();
            Grpc = new GrpcClientInstrumentationOptions();
            AspNetCore = new AspNetCoreInstrumentationOptions();
        }
        public TracesMode Mode { get; set; }
        public SqlClientInstrumentationOptions Sql { get; set; }
        public MySqlDataInstrumentationOptions MySql { get; set; }
        public EntityFrameworkInstrumentationOptions EFCore { get; set; }
        public HttpClientInstrumentationOptions Http { get; set; }
        public GrpcClientInstrumentationOptions Grpc { get; set; }
        public AspNetCoreInstrumentationOptions AspNetCore { get; set; }
    }

    public class MetricsOptions
    {
        public MetricsMode Mode { get; set; }
    }
    public class ServiceOptions
    {
        public ServiceOptions()
        {
            Name = Assembly.GetExecutingAssembly().GetName().Name?.ToString();
            Version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "unknown";
        }
        public string? Name { get; set; }
        public string Version { get; set; }
    }

    public class LoggingOptions
    {
        public LoggingOptions()
        {
            OtelSerilog = new OtelSerilog();
        }
        public LoggingMode Mode { get; set; }
        public OtelSerilog OtelSerilog { get; set; }
    }

    public class OtelSerilog
    {
        public string? Path { get; set; }
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

