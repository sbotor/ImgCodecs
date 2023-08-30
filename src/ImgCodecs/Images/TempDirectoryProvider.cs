using ImgCodecs.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ImgCodecs.Images;

public interface ITempDirectoryProvider
{
    string SupplyPathForEncoded(string originalFilePath, string targetExtension);
    string SupplyPathForDecoded(string originalFilePath);
}

public sealed class TempDirectoryProvider : ITempDirectoryProvider, IDisposable
{
    private readonly ILogger<TempDirectoryProvider> _logger;
    private const string EncodedPrefix = "ENC_";
    private const string DecodedPrefix = "DEC_";
    
    private bool _disposed;

    private readonly List<string> _tempPaths = new();
    private readonly DirectorySettings _settings;
    
    public TempDirectoryProvider(
        IOptions<DirectorySettings> options,
        ILogger<TempDirectoryProvider> logger)
    {
        _logger = logger;
        _settings = options.Value;
    }

    public string SupplyPathForEncoded(string originalFilePath, string targetExtension)
    {
        ThrowIfDisposed();
        var filename = Path.GetFileName(originalFilePath);
        return Attach($"{EncodedPrefix}{filename}{targetExtension}");
    }

    public string SupplyPathForDecoded(string originalFilePath)
    {
        ThrowIfDisposed();
        
        var filename = Path.GetFileName(originalFilePath);
        return Attach($"{DecodedPrefix}{filename}");
    }

    private string Attach(string filename)
    {
        Directory.CreateDirectory(_settings.TempDirectoryPath);
        
        var path = Path.Join(_settings.TempDirectoryPath, filename);
        _tempPaths.Add(path);
        
        return path;
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(TempDirectoryProvider));
        }
    }
    
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        Func<string, bool> predicate = _settings.TempCleanupBehavior switch
        {
            TempCleanupBehavior.DeleteAll => _ => true,
            TempCleanupBehavior.PreserveAll => _ => false,
            TempCleanupBehavior.PreserveDecoded => x => !x.StartsWith(DecodedPrefix),
            TempCleanupBehavior.PreserveEncoded => x => !x.StartsWith(EncodedPrefix),
            _ => throw new ArgumentOutOfRangeException()
        };

        CleanFiles(predicate);
        
        _disposed = true;
    }

    private void CleanFiles(Func<string, bool> predicate)
    {
        foreach (var path in _tempPaths)
        {
            try
            {
                if (predicate(path))
                {
                    File.Delete(path);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "Exception when deleting temp file {path}.",
                    path);
            }
        }
    }
}