using ImgCodecs.Codecs.Ffmpeg;
using ImgCodecs.Codecs.Flif;
using ImgCodecs.Codecs.ImageMagick;
using ImgCodecs.Codecs.JpegLs;
using ImgCodecs.Codecs.Jxl;
using ImgCodecs.Configuration;
using ImgCodecs.Images;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ImgCodecs.Codecs;

public interface ICodecFactory
{
    ICodec CreateCodec(BenchmarkType benchmarkType, int threads);
}

public class CodecFactory : ICodecFactory
{
    private readonly IServiceProvider _serviceProvider;

    public CodecFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public ICodec CreateCodec(BenchmarkType benchmarkType, int threads)
    {
        var tempDirProvider = _serviceProvider.GetRequiredService<ITempDirectoryProvider>();
        var processRunner = _serviceProvider.GetRequiredService<IProcessRunner>();

        switch (benchmarkType)
        {
            case BenchmarkType.Jpeg2000:
                return new ImageMagickCodec(
                    benchmarkType,
                    tempDirProvider,
                    processRunner);
            case BenchmarkType.JpegLs:
                var dirOptions = _serviceProvider.GetRequiredService<IOptions<DirectorySettings>>();
                return new JpegLsCodec(dirOptions, processRunner, tempDirProvider);
            case BenchmarkType.JpegXl:
                return new JxlCodec(tempDirProvider, processRunner);
            case BenchmarkType.Flif:
                return new FlifCodec(tempDirProvider, processRunner);
            case BenchmarkType.Hevc
                or BenchmarkType.HevcLossless
                or BenchmarkType.Vvc:
                return new FfmpegCodec(
                    benchmarkType,
                    threads,
                    tempDirProvider,
                    processRunner);
            default:
                throw new InvalidOperationException("Invalid benchmark type.");
        }
    }
}