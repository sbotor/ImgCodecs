using ImgCodecs.Benchmarking;
using ImgCodecs.Codecs;
using ImgCodecs.Configuration;
using ImgCodecs.Images;
using ImgCodecs.Logging;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;

namespace ImgCodecs;

public static class Setup
{
    private const string LoggingTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] <{SourceContext}> {Message:lj}{NewLine}{Exception}";
    
    public static IServiceCollection Register(this IServiceCollection services)
    {
        services.AddScoped<BenchmarkingPipeline>();

        services.AddScoped<IBenchmarkRunner, BenchmarkRunner>();
        services.AddScoped<IResultsSink, CsvFileResultsSink>();
        
        services.AddScoped<ICodecFactory, CodecFactory>();

        services.AddScoped<IImageProvider, ImageProvider>();
        services.AddScoped<ITempDirectoryProvider, TempDirectoryProvider>();
        
        services.AddScoped<IProcessRunner, ProcessRunner>();
        services.AddScoped<IProcessLoggerProvider, ProcessLoggerProvider>();
        services.AddScoped(x =>
        {
            var provider = x.GetRequiredService<IProcessLoggerProvider>();
            return provider.GetLogger();
        });

        services.AddTransient<ICodecRunner, CodecRunner>();

        return services;
    }

    public static IServiceCollection Configure(this IServiceCollection services, BenchmarkingOptions options)
    {
        services.AddOptions<BenchmarkSettings>().Configure(x =>
        {
            x.RunCount = options.RunCount;
            x.WarmupCount = options.WarmupCount;
            x.ImageBatchSize = options.BatchSize;
            x.BenchmarkType = options.BenchmarkType;

            x.Threads = options.Threads < 1
                ? Environment.ProcessorCount
                : options.Threads;
        });

        services.AddOptions<ProcessRunnerSettings>().Configure(x =>
        {
            x.Timeout = TimeSpan.FromMilliseconds(options.Timeout);
        });
        
        services.AddOptions<DirectorySettings>().Configure(x =>
        {
            x.ImageListPath = options.GetEffectiveListPath();
            x.ImagesDirectoryPath = options.GetEffectiveImageDirPath();
            x.TempDirectoryPath = options.GetEffectiveTempDirPath();
            x.ResultsPath = options.GetEffectiveResultsPath();
            x.TempCleanupBehavior = options.TempCleanupBehavior;
        });
        
        services.AddSingleton<IClock, Clock>();

        return services;
    }

    public static IServiceCollection ConfigureLogging(this IServiceCollection services)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console(LogEventLevel.Information, outputTemplate: LoggingTemplate)
            .WriteTo.File("Logs/logs.txt", LogEventLevel.Debug, outputTemplate: LoggingTemplate)
            .CreateLogger();

        services.AddLogging(x => x.AddSerilog(dispose: true));
        
        return services;
    }
}