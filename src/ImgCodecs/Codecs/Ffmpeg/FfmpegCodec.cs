using ImgCodecs.Codecs.Ffmpeg.Unused;
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
        => _benchmarkType switch
        {
            BenchmarkType.Hevc => HevcEncoder(info, false),
            BenchmarkType.HevcLossless => HevcEncoder(info, true),
            BenchmarkType.Vvc => VvcEncoder(info),
            _ => throw InvalidBenchmarkType()
        };

    private ICodecEncoder HevcEncoder(ImageInfo info, bool lossless)
    {
        var tempEncodedFilePath = _tempDirectoryProvider.SupplyPathForEncoded(
            info.File.FullPath,
            FfmpegHelpers.TargetExtension);
        
        var parameters = new HevcParameters(
            info.File.FullPath,
            tempEncodedFilePath,
            _threads)
        {
            Lossless = lossless
        };
        
        return new HevcEncoder(parameters, _processRunner);
    }

    private ICodecEncoder VvcEncoder(ImageInfo info)
    {
        var tempEncodedFilePath = _tempDirectoryProvider.SupplyPathForEncoded(
            info.File.FullPath,
            FfmpegHelpers.TargetExtension);
        
        var parameters = new FfmpegCodecParameters(
            info.File.FullPath,
            tempEncodedFilePath,
            _threads);
        
        return new VvcEncoder(parameters, _processRunner);
    }

    public ICodecCoder CreateDecoder(ImageInfo info, string encodedFilePath)
        => _benchmarkType switch
        {
            BenchmarkType.Hevc
                or BenchmarkType.HevcLossless
                => HevcDecoder(info, encodedFilePath),
            BenchmarkType.Vvc => VvcDecoder(info, encodedFilePath),
            _ => throw InvalidBenchmarkType()
        };

    private ICodecCoder HevcDecoder(ImageInfo info, string encodedFilePath)
    {
        var tempDecodedFilePath = _tempDirectoryProvider.SupplyPathForDecoded(info.File.FullPath);

        var parameters = new FfmpegCodecParameters(
            encodedFilePath,
            tempDecodedFilePath,
            _threads);
        
        return new FfmpegDecoder(parameters, _processRunner);
    }

    private ICodecCoder VvcDecoder(ImageInfo info, string encodedFilePath)
    {
        var tempDecodedFilePath = _tempDirectoryProvider.SupplyPathForDecoded(info.File.FullPath);

        var parameters = new VvcParameters(
            encodedFilePath,
            tempDecodedFilePath,
            _threads,
            info.Entry.Dimensions);
        
        return new FfmpegDecoder(parameters, _processRunner);
    }

    private InvalidOperationException InvalidBenchmarkType()
        => new($"Invalid benchmark type {_benchmarkType} for {nameof(FfmpegCodec)}.");
}