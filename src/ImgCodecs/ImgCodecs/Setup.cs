using ImgCodecs.Benchmarking;
using ImgCodecs.Configuration;
using ImgCodecs.Diagnostics;
using ImgCodecs.Images;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ImgCodecs;

public static class Setup
{
    public static IServiceCollection Register(this IServiceCollection services)
    {
        services.AddScoped<BenchmarkingPipeline>();

        services.AddScoped<IBenchmarkRunner, BenchmarkRunner>();
        services.AddScoped<IResultsSink, CsvFileResultsSink>();
        
        services.AddScoped<ICodecProcessProviderFactory, CodecProcessProviderFactory>();

        services.AddScoped<IImageProvider, ImageProvider>();
        services.AddScoped<ITempDirectoryProvider, TempDirectoryProvider>();

        services.AddTransient<IProcessRunner, ProcessRunner>();

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

        return services;
    }

    public static IServiceCollection ConfigureLogging(this IServiceCollection services, BenchmarkingOptions options)
    {
        services.AddLogging(x =>
        {
            x.AddSimpleConsole(c =>
            {
                c.IncludeScopes = true;
                c.SingleLine = true;
                c.TimestampFormat = "HH:mm:ss ";
            });
            
            x.SetMinimumLevel(options.MinimumLogLevel);
        });

        return services;
    }
}