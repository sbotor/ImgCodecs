namespace ImgCodecs.Codecs;

public interface ICodecCoder : IDisposable
{
    Task<bool> RunAsync(CancellationToken cancellationToken);
}

public interface ICodecEncoder : ICodecCoder
{
    string EncodedPath { get; }
}

public interface ICodec
{
    ICodecEncoder CreateEncoder(string originalFilePath);
    ICodecCoder CreateDecoder(string originalFilePath, string encodedFilePath);
}