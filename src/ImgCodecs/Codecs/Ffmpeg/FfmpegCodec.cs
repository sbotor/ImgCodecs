using ImgCodecs.Configuration;
using ImgCodecs.Images;

namespace ImgCodecs.Codecs.Ffmpeg;

public class FfmpegCodec : ICodec
{
    private readonly BenchmarkType _benchmarkType;

    private readonly int _threads;
    private readonly ITempDirectoryProvider _tempDirectoryProvider;
    private readonly IProcessRunner _processRunner;

    public FfmpegCodec(
        BenchmarkType benchmarkType,
        int threads,
        ITempDirectoryProvider tempDirectoryProvider,
        IProcessRunner processRunner)
    {
        _benchmarkType = benchmarkType;
        _threads = threads;
        _tempDirectoryProvider = tempDirectoryProvider;
        _processRunner = processRunner;
    }

    public ICodecEncoder CreateEncoder(ImageInfo info)
    {
        var tempEncodedFilePath = _tempDirectoryProvider.SupplyPathForEncoded(
            info.File.FullPath,
            FfmpegHelpers.TargetExtension);

        var parameters = new FfmpegCodecParameters(
            info.File.FullPath,
            tempEncodedFilePath,
            _threads);

        switch (_benchmarkType)
        {
            case BenchmarkType.Hevc:
                return new HevcEncoder(new(
                        info.File.FullPath,
                        tempEncodedFilePath,
                        _threads),
                    _processRunner);
            case BenchmarkType.Vvc:
                return new VvcEncoder(new(
                        info.File.FullPath,
                        tempEncodedFilePath,
                        _threads,
                        info.Entry.Dimensions),
                    _processRunner, _tempDirectoryProvider);
            default:
                throw InvalidBenchmarkType();
        }
    }

    public ICodecCoder CreateDecoder(ImageInfo info, string encodedFilePath)
    {
        var tempDecodedFilePath = _tempDirectoryProvider.SupplyPathForDecoded(info.File.FullPath);

        var parameters = new FfmpegCodecParameters(
            encodedFilePath,
            tempDecodedFilePath,
            _threads);

        return new FfmpegDecoder(parameters, _processRunner);
    }

    private InvalidOperationException InvalidBenchmarkType()
        => new($"Invalid benchmark type {_benchmarkType} for {nameof(FfmpegCodec)}.");
}