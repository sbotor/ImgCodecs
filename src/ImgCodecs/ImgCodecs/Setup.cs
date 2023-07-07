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

        services.AddScoped<IImageProvider, ImageProvider>();
        services.AddScoped<IBenchmarkRunner, BenchmarkRunner>();
        services.AddScoped<IResultsSink, CsvFileResultsSink>();

        services.AddScoped<ITempFileProvider, TempFileProvider>();

        services.AddTransient<IProcessRunner, ProcessRunner>();

        // TODO: Everything below has to be generalised
        services.AddScoped<ICodecProcessFactory, ImageMagickProcessFactory>(
            sp => new(BenchmarkType.Jpeg2000, sp.GetRequiredService<ITempFileProvider>()));

        return services;
    }

    public static IServiceCollection Configure(this IServiceCollection services, BenchmarkingOptions options)
    {
        services.AddOptions<BenchmarkSettings>().Configure(x =>
        { 
            x.RunCount = options.RunCount;
            x.WarmupCount = options.WarmupCount;
            x.ImageBatchSize = options.BatchSize;
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