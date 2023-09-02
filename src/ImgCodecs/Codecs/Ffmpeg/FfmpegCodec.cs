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
    
    public ICodecEncoder CreateEncoder(string originalFilePath)
    {
        var tempEncodedFilePath = _tempDirectoryProvider.SupplyPathForEncoded(
            originalFilePath,
            FfmpegHelpers.TargetExtension);

        var parameters = new FfmpegCodecParameters(
            originalFilePath,
            tempEncodedFilePath,
            _threads);

        return new FfmpegEncoder(_benchmarkType, parameters, _processRunner);
    }

    public ICodecCoder CreateDecoder(string originalFilePath, string encodedFilePath)
    {
        var tempDecodedFilePath = _tempDirectoryProvider.SupplyPathForDecoded(originalFilePath);

        var parameters = new FfmpegCodecParameters(
            originalFilePath,
            encodedFilePath,
            _threads);
        
        return new FfmpegDecoder(parameters, _processRunner);
    }
}