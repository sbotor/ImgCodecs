using System.Diagnostics;
using ImgCodecs.Configuration;
using Microsoft.Extensions.Options;

namespace ImgCodecs.Codecs;

public interface ICodecRunner
{
    TimeSpan Timeout { get; set; }
    double LastRunTime { get; }
    Task<bool> RunTimedAsync(ICodecCoder codec, CancellationToken cancellationToken);
}

public class CodecRunner : ICodecRunner
{
    private readonly Stopwatch _stopwatch = new();

    public TimeSpan Timeout { get; set; }
    public double LastRunTime { get; private set; }

    public CodecRunner(IOptions<ProcessRunnerSettings> options)
    {
        Timeout = options.Value.Timeout;
    }

    public async Task<bool> RunTimedAsync(ICodecCoder codec, CancellationToken cancellationToken)
    {
        using var timeoutCts = CreateTimeoutCts();
        using var cts = LinkTokens(timeoutCts, cancellationToken);

        _stopwatch.Restart();
        var success = await codec.RunAsync(cts.Token);
        _stopwatch.Stop();
        LastRunTime = _stopwatch.ElapsedMilliseconds;
        
        return success;
    }
    
    private CancellationTokenSource? CreateTimeoutCts()
        => Timeout > TimeSpan.Zero
            ? new CancellationTokenSource(Timeout)
            : null;

    private static CancellationTokenSource LinkTokens(CancellationTokenSource? timeoutCts,
        CancellationToken outerToken)
        => timeoutCts is null
            ? CancellationTokenSource.CreateLinkedTokenSource(outerToken)
            : CancellationTokenSource.CreateLinkedTokenSource(timeoutCts.Token, outerToken);
}