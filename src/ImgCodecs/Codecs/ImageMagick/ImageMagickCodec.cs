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
    
    public ICodecEncoder CreateEncoder(string originalFilePath)
    {
        var tempEncodedFilePath = _tempDirectoryProvider.SupplyPathForEncoded(originalFilePath, _targetExtension);

        var parameters = new CodecParameters(originalFilePath, tempEncodedFilePath);

        return new ImageMagickCoder(parameters, _processRunner);
    }
    
    public ICodecCoder CreateDecoder(string originalFilePath, string encodedFilePath)
    {
        var tempDecodedFilePath =
            _tempDirectoryProvider.SupplyPathForDecoded(originalFilePath);

        var parameters = new CodecParameters(originalFilePath, tempDecodedFilePath);

        return new ImageMagickCoder(parameters, _processRunner);
    }
}