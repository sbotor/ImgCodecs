namespace ImgCodecs.Codecs.Ffmpeg;

public sealed class VvcEncoder : ICodecEncoder
{
    private readonly FfmpegCodecParameters _parameters;
    private readonly IProcessRunner _processRunner;

    public string EncodedPath => _parameters.DestinationPath;
    
    public VvcEncoder(FfmpegCodecParameters parameters, IProcessRunner processRunner)
    {
        _parameters = parameters;
        _processRunner = processRunner;
    }
    
    public async Task<bool> RunAsync(CancellationToken cancellationToken)
    {
        var startInfo = ProcessHelpers.CreateStartInfo(FfmpegHelpers.FfmpegFilename, new[]
        {
            "-y",
            "-i", _parameters.SourcePath,
            "-c:v", "vvc",
            "-vvenc-params", "profile=main_10_still_picture",
            "-frames:v", "1",
            "-preset", FfmpegHelpers.EncodingPreset,
            "-threads", _parameters.Threads.ToString(),
            EncodedPath
        });
        
        return await _processRunner.RunAsync(startInfo, cancellationToken) == 0;
    }
}