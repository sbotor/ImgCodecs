using CommandLine;
using ImgCodecs.Benchmarking;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ImgCodecs;

public static class Program
{
    public static Task<int> Main(string[] args)
    {
        var task = Parser.Default.ParseArguments<BenchmarkingOptions>(args)
            .MapResult(Run, _ => Task.FromResult(-1));

        return task;
    }

    private static async Task<int> Run(BenchmarkingOptions options)
    {
        var services = new ServiceCollection();

        services.Register()
            .Configure(options)
            .ConfigureLogging(options);

        var provider = services.BuildServiceProvider();

        using var scope = provider.CreateScope();

        var pipeline = scope.ServiceProvider.GetRequiredService<BenchmarkingPipeline>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Startup>>();

        try
        {
            await pipeline.RunAsync();
            return 0;
        }
        catch (Exception e)
        {
            logger.LogCritical(e, "A critical error has occured.");

            var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
            loggerFactory.Dispose();
            
            return -1;
        }
    }

    private class Startup
    {
    }
}