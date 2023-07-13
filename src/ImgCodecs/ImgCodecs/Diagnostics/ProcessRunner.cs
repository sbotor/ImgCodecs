using System.Diagnostics;
using ImgCodecs.Configuration;
using Microsoft.Extensions.Options;

namespace ImgCodecs.Diagnostics;

public interface IProcessRunner
{
    TimeSpan Timeout { get; set; }
    double LastRunTime { get; }
    Task<double> RunTimedAsync(Process process);
}

public class ProcessRunner : IProcessRunner
{
    private readonly Stopwatch _stopwatch = new();

    public TimeSpan Timeout { get; set; }
    public double LastRunTime { get; private set; }

    public ProcessRunner(IOptions<ProcessRunnerSettings> options)
    {
        Timeout = options.Value.Timeout;
    }
    
    public async Task<double> RunTimedAsync(Process process)
    {
        using var cts = Timeout > TimeSpan.Zero
            ? new CancellationTokenSource(Timeout)
            : null;

        _stopwatch.Restart();

        process.Start();
        await process.WaitForExitAsync(cts?.Token ?? CancellationToken.None);

        _stopwatch.Stop();
        LastRunTime = _stopwatch.ElapsedMilliseconds;

        return LastRunTime;
    }
}