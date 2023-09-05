using ImgCodecs.Configuration;
using ImgCodecs.Images;
using Microsoft.Extensions.Options;

namespace ImgCodecs.Codecs.JpegLs;

public class JpegLsCodec : ICodec
{
    private readonly IProcessRunner _processRunner;
    private readonly ITempDirectoryProvider _tempDirProvider;
    private readonly DirectorySettings _settings;
    
    public JpegLsCodec(
        IOptions<DirectorySettings> options,
        IProcessRunner processRunner,
        ITempDirectoryProvider tempDirProvider)
    {
        _processRunner = processRunner;
        _tempDirProvider = tempDirProvider;
        _settings = options.Value;
    }
    
    public ICodecEncoder CreateEncoder(ImageInfo info)
    {
        var tempEncodedFilePath = _tempDirProvider.SupplyPathForEncoded(info.File.FullPath,
            JpegLsHelpers.TargetExtension);
        
        var parameters = new JpegLsParameters(
            info.File.FullPath,
            tempEncodedFilePath,
            _settings.ScriptsDirectoryPath)
        {
            Decode = false
        };

        return new JpegLsCoder(parameters, _processRunner);
    }

    public ICodecCoder CreateDecoder(ImageInfo info, string encodedFilePath)
    {
        var tempEncodedFilePath = _tempDirProvider.SupplyPathForDecoded(info.File.FullPath);
        
        var parameters = new JpegLsParameters(
            info.File.FullPath,
            tempEncodedFilePath,
            _settings.ScriptsDirectoryPath)
        {
            Decode = true
        };

        return new JpegLsCoder(parameters, _processRunner);
    }
}