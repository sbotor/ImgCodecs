namespace ImgCodecs.Codecs.Ffmpeg;

public sealed class FfmpegDecoder : ICodecCoder
{
    private readonly FfmpegCodecParameters _parameters;
    private readonly IProcessRunner _processRunner;

    public FfmpegDecoder(FfmpegCodecParameters parameters, IProcessRunner processRunner)
    {
        _parameters = parameters;
        _processRunner = processRunner;
    }
    
    public async Task<bool> RunAsync(CancellationToken cancellationToken)
    {
        var startInfo = ProcessHelpers.CreateStartInfo(FfmpegHelpers.FfmpegFilename, GetArguments());
        return await _processRunner.RunAsync(startInfo, cancellationToken) == 0;
    }
    
    private IEnumerable<string> GetArguments()
        => new[]
        {
            "-y",
            "-i", _parameters.SourcePath,
            "-frames:v", "1",
            "-threads", _parameters.Threads.ToString(),
            _parameters.DestinationPath
        };
}