using ImgCodecs.Configuration;
using ImgCodecs.Images;

namespace ImgCodecs.Codecs.ImageMagick;

public class ImageMagickCodec : ICodec
{
    private readonly string _targetExtension;
    private readonly ITempDirectoryProvider _tempDirectoryProvider;
    private readonly IProcessRunner _processRunner;

    public ImageMagickCodec(
        BenchmarkType benchmarkType,
        ITempDirectoryProvider tempDirectoryProvider,
        IProcessRunner processRunner)
    {
        _targetExtension = ImageMagickHelpers.GetExtension(benchmarkType);
        _tempDirectoryProvider = tempDirectoryProvider;
        _processRunner = processRunner;
    }
    
    public ICodecEncoder CreateEncoder(ImageInfo info)
    {
        var tempEncodedFilePath = _tempDirectoryProvider.SupplyPathForEncoded(info.File.FullPath, _targetExtension);

        var parameters = new CodecParameters(info.File.FullPath, tempEncodedFilePath);

        return new ImageMagickCoder(parameters, _processRunner);
    }
    
    public ICodecCoder CreateDecoder(ImageInfo info, string encodedFilePath)
    {
        var tempDecodedFilePath =
            _tempDirectoryProvider.SupplyPathForDecoded(info.File.FullPath);

        var parameters = new CodecParameters(info.File.FullPath, tempDecodedFilePath);

        return new ImageMagickCoder(parameters, _processRunner);
    }
}