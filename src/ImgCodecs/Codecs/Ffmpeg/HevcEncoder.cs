namespace ImgCodecs.Codecs.Ffmpeg;

public class HevcParameters : FfmpegCodecParameters
{
    public bool Lossless { get; init; }
    
    public HevcParameters(string sourcePath, string destinationPath, int threads)
        : base(sourcePath, destinationPath, threads)
    {
    }
}

public sealed class HevcEncoder : ICodecEncoder
{
    private readonly HevcParameters _parameters;
    private readonly IProcessRunner _processRunner;

    public string EncodedPath => _parameters.DestinationPath;

    public HevcEncoder(HevcParameters parameters, IProcessRunner processRunner)
    {
        _parameters = parameters;
        _processRunner = processRunner;
    }
    
    public async Task<bool> RunAsync(CancellationToken cancellationToken)
    {
        var lossless = _parameters.Lossless
            ? "lossless=1"
            : "lossless=0";
        
        var startInfo = ProcessHelpers.CreateStartInfo(FfmpegHelpers.FfmpegFilename, new[]
        {
            "-y",
            "-i", _parameters.SourcePath,
            "-c:v", "libx265",
            "-x265-params", lossless,
            "-profile:v", "main444-stillpicture",
            "-frames:v", "1",
            "-preset", FfmpegHelpers.EncodingPreset,
            "-threads", _parameters.Threads.ToString(),
            EncodedPath
        });
        
        return await _processRunner.RunAsync(startInfo, cancellationToken) == 0;
    }
}