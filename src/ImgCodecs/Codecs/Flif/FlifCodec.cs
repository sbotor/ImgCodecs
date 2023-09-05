using ImgCodecs.Images;

namespace ImgCodecs.Codecs.Flif;

public class FlifCodec : ICodec
{
    private readonly ITempDirectoryProvider _tempDirProvider;
    private readonly IProcessRunner _processRunner;

    public FlifCodec(ITempDirectoryProvider tempDirProvider, IProcessRunner processRunner)
    {
        _processRunner = processRunner;
        _tempDirProvider = tempDirProvider;
    }
    
    public ICodecEncoder CreateEncoder(ImageInfo info)
    {
        var tempEncodedFilePath = _tempDirProvider.SupplyPathForEncoded(info.File.FullPath,
            FlifHelpers.TargetExtension);
        
        var parameters = new FlifParameters(info.File.FullPath, tempEncodedFilePath)
        {
            Decode = false
        };

        return new FlifCoder(parameters, _processRunner);
    }

    public ICodecCoder CreateDecoder(ImageInfo info, string encodedFilePath)
    {
        var tempDecodedFilePath = _tempDirProvider.SupplyPathForDecoded(info.File.FullPath);
        
        var parameters = new FlifParameters(encodedFilePath, tempDecodedFilePath)
        {
            Decode = true
        };

        return new FlifCoder(parameters, _processRunner);
    }
}