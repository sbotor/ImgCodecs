using ImgCodecs.Configuration;
using Microsoft.Extensions.Options;
using Serilog;

namespace ImgCodecs.Logging;

public interface ICodecLoggerFactory : IDisposable
{
    ICodecLogger GetLogger();
}

public class CodecLoggerFactory : ICodecLoggerFactory
{
    private readonly IClock _clock;
    private readonly string _codecName;
    
    private CodecLogger? _logger;
    
    private bool _disposed;

    public CodecLoggerFactory(IClock clock, IOptions<BenchmarkSettings> options)
    {
        _clock = clock;
        _codecName = options.Value.BenchmarkType.ToString();
    }
    
    public ICodecLogger GetLogger()
    {
        ThrowIfDisposed();

        return _logger ??= CreateLogger();
    }

    private CodecLogger CreateLogger()
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
            throw new ObjectDisposedException(nameof(CodecLoggerFactory));
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