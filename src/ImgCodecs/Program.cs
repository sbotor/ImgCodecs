using CommandLine;
using ImgCodecs.Benchmarking;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ImgCodecs;

public enum ReturnValue
{
    ParsingError = -2,
    Error = -1,
    Ok = 0,
    Canceled = 1
}

public static class Program
{
    private static readonly CancellationTokenSource Cts = new();
    
    public static async Task<int> Main(string[] args)
    {
        var task = Parser.Default.ParseArguments<BenchmarkingOptions>(args)
            .MapResult(Run, _ => Task.FromResult(ReturnValue.ParsingError));

        return (int)await task;
    }

    private static async Task<ReturnValue> Run(BenchmarkingOptions options)
    {
        Console.CancelKeyPress += (_, args) =>
        {
            Cts.Cancel();
            args.Cancel = true;
        };
        
        var services = new ServiceCollection();

        services.Register()
            .Configure(options)
            .ConfigureLogging();

        await using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();

        var diagHelper = new DiagnosticsHelper(
            scope.ServiceProvider.GetRequiredService<ILogger<DiagnosticsHelper>>());

        diagHelper.LogOptions(options);

        var pipeline = scope.ServiceProvider.GetRequiredService<BenchmarkingPipeline>();

        try
        {
            await pipeline.RunAsync(Cts.Token);
            return ReturnValue.Ok;
        }
        catch (OperationCanceledException)
        {
            diagHelper.LogCancellation();
            return ReturnValue.Canceled;
        }
        catch (Exception e)
        {
            diagHelper.LogCritical(e);

            return ReturnValue.Error;
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

        public void LogCancellation()
        {
            _logger.LogWarning("The run was cancelled.");
        }
    }
}