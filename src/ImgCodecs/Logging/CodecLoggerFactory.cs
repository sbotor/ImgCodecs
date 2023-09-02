using ImgCodecs.Configuration;
using Microsoft.Extensions.Options;
using Serilog;

namespace ImgCodecs.Logging;

public interface IProcessLoggerProvider : IDisposable
{
    IProcessLogger GetLogger();
}

public class ProcessLoggerProvider : IProcessLoggerProvider
{
    private readonly IClock _clock;
    private readonly string _codecName;
    
    private ProcessLogger? _logger;
    
    private bool _disposed;

    public ProcessLoggerProvider(IClock clock, IOptions<BenchmarkSettings> options)
    {
        _clock = clock;
        _codecName = options.Value.BenchmarkType.ToString();
    }
    
    public IProcessLogger GetLogger()
    {
        ThrowIfDisposed();

        return _logger ??= CreateLogger();
    }

    private ProcessLogger CreateLogger()
    {
        var now = _clock.Now.ToString("yyyy-MM-dd-HH-mm-ss");
        var filenameOut = $"Logs/{now}_{_codecName}_out.txt";
        var filenameErr = $"Logs/{now}_{_codecName}_err.txt";
        
        var configOut = new LoggerConfiguration()
            .WriteTo.File(filenameOut);
        var configErr = new LoggerConfiguration()
            .WriteTo.File(filenameErr);

        return new(configOut.CreateLogger(), configErr.CreateLogger());
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(ProcessLoggerProvider));
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _logger?.Dispose();
        
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}