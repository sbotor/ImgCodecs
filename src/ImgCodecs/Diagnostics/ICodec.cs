using System.Diagnostics;

namespace ImgCodecs.Diagnostics;

public interface ICodec
{
    EncodingInfo CreateEncoder(string originalFilePath);
    Process CreateDecoder(string originalFilePath, string encodedFilePath);
}

public sealed record EncodingInfo(
    Process Process,
    string EncodedPath) : IDisposable
{
    private bool _disposed;
    
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }
        
        Process.Dispose();
        _disposed = true;
    }
}