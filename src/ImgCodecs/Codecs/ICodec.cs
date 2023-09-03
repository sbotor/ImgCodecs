using ImgCodecs.Images;

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
    ICodecEncoder CreateEncoder(ImageInfo info);
    ICodecCoder CreateDecoder(ImageInfo info, string encodedFilePath);
}

public record ImageInfo(ImageEntry Entry, ImageFileInfo File);