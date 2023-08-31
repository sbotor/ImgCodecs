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
            .ConfigureLogging();

        var provider = services.BuildServiceProvider();

        using var scope = provider.CreateScope();

        var pipeline = scope.ServiceProvider.GetRequiredService<BenchmarkingPipeline>();
        var diagHelper = new DiagnosticsHelper(
            scope.ServiceProvider.GetRequiredService<ILogger<DiagnosticsHelper>>());
        
        diagHelper.LogOptions(options);

        try
        {
            await pipeline.RunAsync();
            return 0;
        }
        catch (Exception e)
        {
            diagHelper.LogCritical(e);
            
            return -1;
        }
    }

    private class DiagnosticsHelper
    {
        private readonly ILogger<DiagnosticsHelper> _logger;

        public DiagnosticsHelper(ILogger<DiagnosticsHelper> logger)
        {
            _logger = logger;
        }

        public void LogOptions(BenchmarkingOptions options)
        {
            _logger.LogDebug("Program started. Options:{newline}{@options}",
                Environment.NewLine, options);
        }

        public void LogCritical(Exception exception)
        {
            _logger.LogCritical(exception, "A critical error has occured.");
        }
    }
}