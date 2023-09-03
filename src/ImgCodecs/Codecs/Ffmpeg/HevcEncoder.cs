namespace ImgCodecs.Codecs.Ffmpeg;

public sealed class HevcEncoder : ICodecEncoder
{
    private readonly FfmpegCodecParameters _parameters;
    private readonly IProcessRunner _processRunner;

    public string EncodedPath => _parameters.DestinationPath;

    public HevcEncoder(FfmpegCodecParameters parameters, IProcessRunner processRunner)
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
            "-c:v", "libx265",
            "-x265-params", "lossless=1",
            "-profile:v", "main444-stillpicture",
            "-frames:v", "1",
            "-threads", _parameters.Threads.ToString(),
            EncodedPath
        });
        
        return await _processRunner.RunAsync(startInfo, cancellationToken) == 0;
    }
    
    public void Dispose()
    {
    }
}