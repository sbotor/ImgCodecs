namespace ImgCodecs.Codecs.Jxl;

public sealed class JxlEncoder : ICodecEncoder
{
    private readonly CodecParameters _parameters;
    private readonly IProcessRunner _processRunner;

    public JxlEncoder(CodecParameters parameters, IProcessRunner processRunner)
    {
        _parameters = parameters;
        _processRunner = processRunner;
    }

    public string EncodedPath => _parameters.DestinationPath;

    public async Task<bool> RunAsync(CancellationToken cancellationToken)
    {
        var starInfo = ProcessHelpers.CreateStartInfo(JxlHelpers.EncoderFilename, new[]
        {
            _parameters.SourcePath,
            "-q", "100",
            _parameters.DestinationPath
        });

        return await _processRunner.RunAsync(starInfo, cancellationToken) == 0;
    }
}