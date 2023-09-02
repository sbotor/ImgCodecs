using System.Diagnostics;
using ImgCodecs.Logging;
using Microsoft.Extensions.Logging;

namespace ImgCodecs.Codecs;

public interface IProcessRunner
{
    Task<int?> RunAsync(ProcessStartInfo startInfo, CancellationToken cancellationToken);
}

public class ProcessRunner : IProcessRunner
{
    private readonly ILogger<ProcessRunner> _logger;
    private readonly IProcessLogger _processLogger;

    public ProcessRunner(ILogger<ProcessRunner> logger, IProcessLogger processLogger)
    {
        _logger = logger;
        _processLogger = processLogger;
    }

    public async Task<int?> RunAsync(ProcessStartInfo startInfo, CancellationToken cancellationToken)
    {
        using var process = new Process();
        process.StartInfo = startInfo;

        return await RunAsync(process, cancellationToken);
    }
    
    private async Task<int?> RunAsync(Process process, CancellationToken cancellationToken)
    {
        var processCommand = GetProcessCommand(process);
        
        try
        {
            _logger.LogDebug("Starting process '{cmd}'.", processCommand);
            
            process.Start();
            await process.WaitForExitAsync(cancellationToken);
            
            _logger.LogDebug("Finished process '{cmd}'. Exit code: {exitCode}",
                processCommand, process.ExitCode);

            await LogOutputs(process);
            
            return process.ExitCode;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception e)
        {
            _logger.LogWarning(e, "Error running process '{cmd}'", processCommand);
            return null;
        }
    }
    
    private async Task LogOutputs(Process process)
    {
        try
        {
            await _processLogger.LogOutput(process.StandardOutput);
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Could not log standard output.");
        }

        try
        {
            await _processLogger.LogError(process.StandardError);
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Could not log standard error.");
        }
    }

    private static string GetProcessCommand(Process process)
    {
        var startInfo = process.StartInfo;
        var args = !string.IsNullOrEmpty(startInfo.Arguments)
            ? startInfo.Arguments
            : string.Join(' ', startInfo.ArgumentList);

        return $"{process.ProcessName} {args}";
    }
}