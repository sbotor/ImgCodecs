using ImgCodecs.Images;

namespace ImgCodecs.Codecs.Ffmpeg.Unused;

public sealed class OpenVvcDecoder : ICodecCoder
{
    private readonly VvcParameters _parameters;
    private readonly IProcessRunner _processRunner;
    private readonly ITempDirectoryProvider _tempDirProvider;

    public OpenVvcDecoder(
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
        var yuvPath = await Decode(cancellationToken);
        if (yuvPath is null)
        {
            return false;
        }

        return await TransformYuv(yuvPath, cancellationToken);
    }

    private async Task<string?> Decode(CancellationToken cancellationToken)
    {
        var outputPath = _tempDirProvider.SupplyPathForDecoded(
            _parameters.SourcePath,
            FfmpegHelpers.YuvExtension);
        
        var startInfo = ProcessHelpers.CreateStartInfo(FfmpegHelpers.OpenVvcFilename, new[]
        {
            "-i", _parameters.SourcePath,
            "-o", outputPath
        });

        return await _processRunner.RunAsync(startInfo, cancellationToken) == 0
            ? outputPath
            : null;
    }

    private async Task<bool> TransformYuv(string yuvPath, CancellationToken cancellationToken)
    {
        var startInfo = ProcessHelpers.CreateStartInfo(FfmpegHelpers.FfmpegFilename, new[]
        {
            "-y",
            "-s", _parameters.Dimensions.ToString(),
            "-i", yuvPath,
            "-frames:v", "1",
            _parameters.DestinationPath
        });

        return await _processRunner.RunAsync(startInfo, cancellationToken) == 0;
    }
}