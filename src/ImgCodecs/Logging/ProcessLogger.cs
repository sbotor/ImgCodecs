using Serilog.Core;

namespace ImgCodecs.Logging;

public interface IProcessLogger
{
    public Task LogOutput(StreamReader reader);
    public Task LogError(StreamReader reader);
}

public sealed class ProcessLogger : IProcessLogger, IDisposable
{
    private readonly Logger _stdOut;
    private readonly Logger _stdErr;
    
    private bool _disposed;

    public ProcessLogger(Logger stdOut, Logger stdErr)
    {
        _stdOut = stdOut;
        _stdErr = stdErr;
    }
    
    public async Task LogOutput(StreamReader reader)
    {
        var str = await reader.ReadToEndAsync();

        if (string.IsNullOrEmpty(str))
        {
            return;
        }

        _stdOut.Information(str);
    }

    public async Task LogError(StreamReader reader)
    {
        var str = await reader.ReadToEndAsync();

        if (string.IsNullOrEmpty(str))
        {
            return;
        }

        _stdErr.Error(str);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _stdOut.Dispose();
        _stdErr.Dispose();
        _disposed = true;
    }
}