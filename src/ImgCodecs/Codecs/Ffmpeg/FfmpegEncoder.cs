using ImgCodecs.Configuration;

namespace ImgCodecs.Codecs.Ffmpeg;

public sealed class FfmpegEncoder : ICodecEncoder
{
    private readonly BenchmarkType _benchmarkType;
    private readonly FfmpegCodecParameters _parameters;
    private readonly IProcessRunner _processRunner;

    public string EncodedPath => _parameters.DestinationPath;

    public FfmpegEncoder(BenchmarkType benchmarkType, FfmpegCodecParameters parameters, IProcessRunner processRunner)
    {
        _benchmarkType = benchmarkType;
        _parameters = parameters;
        _processRunner = processRunner;
    }
    
    public async Task<bool> RunAsync(CancellationToken cancellationToken)
    {
        var startInfo = ProcessHelpers.CreateStartInfo(FfmpegHelpers.ProcFilename, GetArguments());
        return await _processRunner.RunAsync(startInfo, cancellationToken) == 0;
    }
    
    public void Dispose()
    {
    }

    private IEnumerable<string> GetArguments()
        => _benchmarkType == BenchmarkType.Hevc
            ? new[]
            {
                "-y",
                "-i", _parameters.SourcePath,
                "-c:v", "libx265",
                "-x265-params", "lossless=1",
                "-frames:v", "1",
                "-threads", _parameters.Threads.ToString(),
                EncodedPath
            }
            : new[]
            {
                "-y",
                "-i", _parameters.SourcePath,
                "-c:v", "vvc",
                "-frames:v", "1",
                "-threads", _parameters.Threads.ToString(),
                EncodedPath
            };
}