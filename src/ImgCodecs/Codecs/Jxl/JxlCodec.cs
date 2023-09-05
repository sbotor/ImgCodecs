using ImgCodecs.Images;

namespace ImgCodecs.Codecs.Jxl;

public class JxlCodec : ICodec
{
    private readonly ITempDirectoryProvider _tempDirProvider;
    private readonly IProcessRunner _processRunner;

    public JxlCodec(ITempDirectoryProvider tempDirProvider, IProcessRunner processRunner)
    {
        _tempDirProvider = tempDirProvider;
        _processRunner = processRunner;
    }
    
    public ICodecEncoder CreateEncoder(ImageInfo info)
    {
        var tempEncodedFilePath = _tempDirProvider.SupplyPathForEncoded(info.File.FullPath,
            JxlHelpers.TargetExtension);
        
        var parameters = new CodecParameters(info.File.FullPath, tempEncodedFilePath);

        return new JxlEncoder(parameters, _processRunner);
    }

    public ICodecCoder CreateDecoder(ImageInfo info, string encodedFilePath)
    {
        var tempDecodedFilePath = _tempDirProvider.SupplyPathForDecoded(info.File.FullPath);

        var parameters = new CodecParameters(encodedFilePath, tempDecodedFilePath);

        return new JxlDecoder(parameters, _processRunner);
    }
}