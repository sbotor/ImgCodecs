using ImgCodecs.Codecs.Ffmpeg;
using ImgCodecs.Codecs.ImageMagick;
using ImgCodecs.Configuration;
using ImgCodecs.Images;
using Microsoft.Extensions.Options;

namespace ImgCodecs.Codecs;

public interface ICodecFactory
{
    ICodec CreateCodec(BenchmarkType benchmarkType, int threads);
}

public class CodecFactory : ICodecFactory
{
    private readonly ITempDirectoryProvider _tempDirectoryProvider;
    private readonly IProcessRunner _processRunner;

    public CodecFactory(
        ITempDirectoryProvider tempDirectoryProvider,
        IProcessRunner processRunner)
    {
        _tempDirectoryProvider = tempDirectoryProvider;
        _processRunner = processRunner;
    }

    public ICodec CreateCodec(BenchmarkType benchmarkType, int threads)
        => benchmarkType switch
        {
            BenchmarkType.Flif
                or BenchmarkType.Jpeg2000
                or BenchmarkType.JpegLs
                or BenchmarkType.JpegXl
                => new ImageMagickCodec(
                    benchmarkType,
                    _tempDirectoryProvider,
                    _processRunner),

            BenchmarkType.Hevc
                or BenchmarkType.Vvc
                => new FfmpegCodec(
                    benchmarkType,
                    threads,
                    _tempDirectoryProvider,
                    _processRunner),

            _ => throw new InvalidOperationException("Invalid benchmark type.")
        };
}