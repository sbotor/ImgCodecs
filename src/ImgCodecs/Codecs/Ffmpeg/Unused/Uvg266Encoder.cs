using ImgCodecs.Images;

namespace ImgCodecs.Codecs.Ffmpeg.Unused;

public class VvcParameters : FfmpegCodecParameters
{
    public ImageDimensions Dimensions { get; }

    public VvcParameters(
        string sourcePath,
        string destinationPath,
        int threads,
        ImageDimensions dimensions)
        : base(sourcePath, destinationPath, threads)
    {
        Dimensions = dimensions;
    }
}

public sealed class Uvg266Encoder : ICodecEncoder
{
    private readonly VvcParameters _parameters;
    private readonly IProcessRunner _processRunner;
    private readonly ITempDirectoryProvider _tempDirProvider;

    public string EncodedPath => _parameters.DestinationPath;
    
    public Uvg266Encoder(
        VvcParameters parameters,
        IProcessRunner processRunner,
        ITempDirectoryProvider tempDirProvider)
    {
        _parameters = parameters;
        _processRunner = processRunner;
        _tempDirProvider = tempDirProvider;
    }

    public async Task<bool> RunAsync(CancellationToken cancellationToken)
    {
        var yuvPath = await CreateYuv(cancellationToken);
        if (yuvPath is null)
        {
            return false;
        }
        
        return await Encode(yuvPath, cancellationToken);
    }

    private async Task<string?> CreateYuv(CancellationToken cancellationToken)
    {
        var outputPath = _tempDirProvider.SupplyPathForEncoded(
            _parameters.SourcePath,
            FfmpegHelpers.YuvExtension);
        
        var startInfo = ProcessHelpers.CreateStartInfo(FfmpegHelpers.FfmpegFilename, new[]
        {
            "-y",
            "-i", _parameters.SourcePath,
            "-c:v", "rawvideo",
            "-pix_fmt", "yuv420p",
            outputPath
        });

        var exitCode = await _processRunner.RunAsync(startInfo, cancellationToken);
        return exitCode == 0 ? outputPath : null;
    }

    private async Task<bool> Encode(string sourcePath, CancellationToken cancellationToken)
    {
        var startInfo = ProcessHelpers.CreateStartInfo(FfmpegHelpers.Uvg266Filename, new[]
        {
            "--input", sourcePath,
            "--input-res", _parameters.Dimensions.ToString(),
            "--frames", "1",
            "--lossless",
            "--output", _parameters.DestinationPath
        });

        return await _processRunner.RunAsync(startInfo, cancellationToken) == 0;
    }
}