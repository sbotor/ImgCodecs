using ImgCodecs.Configuration;
using ImgCodecs.Images;

namespace ImgCodecs.Diagnostics;

public interface ICodecFactory
{
    ICodec GetProvider(BenchmarkType benchmarkType);
}

public class CodecFactory : ICodecFactory
{
    private readonly ITempDirectoryProvider _tempDirectoryProvider;

    public CodecFactory(ITempDirectoryProvider tempDirectoryProvider)
    {
        _tempDirectoryProvider = tempDirectoryProvider;
    }

    public ICodec GetProvider(BenchmarkType benchmarkType)
        => benchmarkType switch
        {
            BenchmarkType.Flif
                or BenchmarkType.Jpeg2000
                or BenchmarkType.JpegLs
                or BenchmarkType.JpegXl
                => new ImageMagickCodec(benchmarkType, _tempDirectoryProvider),

            BenchmarkType.Hevc
                or BenchmarkType.Vvc
                => new FfmpegCodec(benchmarkType, _tempDirectoryProvider),

            _ => throw new InvalidOperationException("Invalid benchmark type.")
        };
}