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

    public CodecFactory(ITempDirectoryProvider tempDirectoryProvider)
    {
        _tempDirectoryProvider = tempDirectoryProvider;
    }

    public ICodec CreateCodec(BenchmarkType benchmarkType, int threads)
        => benchmarkType switch
        {
            BenchmarkType.Flif
                or BenchmarkType.Jpeg2000
                or BenchmarkType.JpegLs
                or BenchmarkType.JpegXl
                => new ImageMagickCodec(benchmarkType, _tempDirectoryProvider),

            BenchmarkType.Hevc
                or BenchmarkType.Vvc
                => new FfmpegCodec(benchmarkType, threads, _tempDirectoryProvider),

            _ => throw new InvalidOperationException("Invalid benchmark type.")
        };
}