using ImgCodecs.Configuration;
using ImgCodecs.Images;

namespace ImgCodecs.Diagnostics;

public interface ICodecProcessProviderFactory
{
    ICodecProcessProvider GetProvider(BenchmarkType benchmarkType);
}

public class CodecProcessProviderFactory : ICodecProcessProviderFactory
{
    private readonly ITempDirectoryProvider _tempDirectoryProvider;

    public CodecProcessProviderFactory(ITempDirectoryProvider tempDirectoryProvider)
    {
        _tempDirectoryProvider = tempDirectoryProvider;
    }

    public ICodecProcessProvider GetProvider(BenchmarkType benchmarkType)
        => benchmarkType switch
        {
            BenchmarkType.Flif
                or BenchmarkType.Jpeg2000
                or BenchmarkType.JpegLs
                or BenchmarkType.JpegXl
                => new ImageMagickProcessProvider(benchmarkType, _tempDirectoryProvider),

            BenchmarkType.Hevc
                or BenchmarkType.Vvc
                => new FfmpegProcessProvider(benchmarkType, _tempDirectoryProvider),

            _ => throw new InvalidOperationException("Invalid benchmark type.")
        };
}