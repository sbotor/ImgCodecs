namespace ImgCodecs.Codecs.ImageMagick;

public sealed class ImageMagickCoder : ICodecEncoder
{
    private readonly CodecParameters _parameters;
    private readonly IProcessRunner _processRunner;

    public string EncodedPath => _parameters.DestinationPath;

    public ImageMagickCoder(CodecParameters parameters, IProcessRunner processRunner)
    {
        _parameters = parameters;
        _processRunner = processRunner;
    }

    public async Task<bool> RunAsync(CancellationToken cancellationToken)
    {
        var startInfo = ProcessHelpers.CreateStartInfo(ImageMagickHelpers.MagickFilename, new[]
        {
            _parameters.SourcePath,
            _parameters.DestinationPath
        });
        
        return await _processRunner.RunAsync(startInfo, cancellationToken) == 0;
    }
}
