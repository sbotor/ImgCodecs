using ImgCodecs.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ImgCodecs.Images;

public interface ITempDirectoryProvider
{
    string SupplyPathForEncoded(string originalFilePath, string targetExtension);
    string SupplyPathForDecoded(string originalFilePath);
    string SupplyPathForDecoded(string originalFilePath, string targetExtension);
}

public sealed class TempDirectoryProvider : ITempDirectoryProvider, IDisposable
{
    private const string EncodedPrefix = "ENC_";
    private const string DecodedPrefix = "DEC_";
    private const int CountThreshold = 20;
    
    private readonly List<string> _tempPaths = new();
    private readonly DirectorySettings _settings;
    private readonly ILogger<TempDirectoryProvider> _logger;
    private readonly Func<string, bool> _cleanupPredicate;
    
    private bool _disposed;
    
    public TempDirectoryProvider(
        IOptions<DirectorySettings> options,
        ILogger<TempDirectoryProvider> logger)
    {
        _logger = logger;
        _settings = options.Value;
        _cleanupPredicate = GetCleanupPredicate();
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

    public string SupplyPathForDecoded(string originalFilePath, string targetExtension)
    {
        ThrowIfDisposed();

        var filename = Path.GetFileName(originalFilePath);
        return Attach($"{DecodedPrefix}{filename}{targetExtension}");
    }

    private string Attach(string filename)
    {
        if (_tempPaths.Count == CountThreshold)
        {
            CleanFiles();
        }
        
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
        
        CleanFiles();
        
        _disposed = true;
    }

    private void CleanFiles()
    {
        foreach (var path in _tempPaths)
        {
            try
            {
                if (_cleanupPredicate(path))
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
        
        _tempPaths.Clear();
    }
    
    private Func<string, bool> GetCleanupPredicate()
        => _settings.TempCleanupBehavior switch
        {
            TempCleanupBehavior.DeleteAll => _ => true,
            TempCleanupBehavior.PreserveAll => _ => false,
            TempCleanupBehavior.PreserveDecoded => x => !x.StartsWith(DecodedPrefix),
            TempCleanupBehavior.PreserveEncoded => x => !x.StartsWith(EncodedPrefix),
            _ => throw new InvalidOperationException("Invalid temp cleanup behavior.")
        };
}