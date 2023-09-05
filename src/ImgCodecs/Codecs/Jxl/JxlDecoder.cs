namespace ImgCodecs.Codecs.Jxl;

public class JxlDecoder : ICodecCoder
{
    private readonly CodecParameters _parameters;
    private readonly IProcessRunner _processRunner;

    public JxlDecoder(CodecParameters parameters, IProcessRunner processRunner)
    {
        _parameters = parameters;
        _processRunner = processRunner;
    }

    public async Task<bool> RunAsync(CancellationToken cancellationToken)
    {
        var starInfo = ProcessHelpers.CreateStartInfo(JxlHelpers.DecoderFilename, new[]
        {
            _parameters.SourcePath,
            _parameters.DestinationPath
        });

        return await _processRunner.RunAsync(starInfo, cancellationToken) == 0;
    }
}