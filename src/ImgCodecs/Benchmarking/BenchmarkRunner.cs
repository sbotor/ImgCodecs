using ImgCodecs.Configuration;
using ImgCodecs.Diagnostics;
using ImgCodecs.Images;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ImgCodecs.Benchmarking;

public interface IBenchmarkRunner
{
    Task<IReadOnlyCollection<BenchmarkImageResults>> RunBatchAsync(IReadOnlyCollection<ImageEntry> images);
}

public class BenchmarkRunner : IBenchmarkRunner
{
    private readonly BenchmarkSettings _settings;
    private readonly ICodec _codec;
    private readonly IImageProvider _imageProvider;

    private readonly IProcessRunner _processRunner;
    private readonly ILogger<BenchmarkRunner> _logger;

    public BenchmarkRunner(IOptions<BenchmarkSettings> options,
        ICodecFactory codecFactory,
        IImageProvider imageProvider,
        IProcessRunner processRunner,
        ILogger<BenchmarkRunner> logger)
    {
        _settings = options.Value;
        _codec = codecFactory.GetProvider(_settings.BenchmarkType);
        _imageProvider = imageProvider;
        _processRunner = processRunner;
        _logger = logger;
    }
    
    public async Task<IReadOnlyCollection<BenchmarkImageResults>> RunBatchAsync(IReadOnlyCollection<ImageEntry> images)
    {
        _logger.LogDebug("Started processing a batch of {count} images.", images.Count);
        
        var list = new List<BenchmarkImageResults>(images.Count);
        foreach (var entry in images)
        {
            var result = await RunForImageAsync(entry);
            list.Add(result);
            
            _logger.LogDebug("Processed image {name}.", entry.Name);
        }

        return list;
    }

    private async Task<BenchmarkImageResults> RunForImageAsync(ImageEntry image)
    {
        var originalInfo = _imageProvider.GetInfoFromFilename(image.Name);
        var results = new BenchmarkImageResults(image, originalInfo.ByteCount);
        
        _logger.LogDebug("Processing image {name}.", image.Name);
        
        for (var i = 0; i < _settings.WarmupCount; i++)
        {
            var encodedPath = await MeasureEncoding(originalInfo.FullPath);
            if (encodedPath is null)
            {
                continue;
            }
            
            await MeasureDecoding(originalInfo.FullPath, encodedPath);
        }

        for (var i = 0; i < _settings.RunCount; i++)
        {
            var encodedPath = await MeasureEncoding(originalInfo.FullPath);
            if (encodedPath is null)
            {
                continue;
            }
            
            var encodingTime = _processRunner.LastRunTime;
            var encodedInfo = _imageProvider.GetInfoFromPath(encodedPath);
            
            var decodedSuccessfully = await MeasureDecoding(originalInfo.FullPath, encodedPath);
            if (!decodedSuccessfully)
            {
                continue;
            }
            
            var decodingTime = _processRunner.LastRunTime;
            
            results.Collect(new(
                encodingTime,
                decodingTime,
                encodedInfo.ByteCount));
        }

        return results;
    }

    private async Task<string?> MeasureEncoding(string path)
    {
        using var encoder = _codec.CreateEncoder(path);

        await _processRunner.RunTimedAsync(encoder.Process);

        if (encoder.Process.ExitCode == 0)
        {
            return encoder.EncodedPath;
        }
        
        _logger.LogError("Could not encode file {path}. Exit code: {code}.",
            path, encoder.Process.ExitCode);
        return null;
    }

    private async Task<bool> MeasureDecoding(string originalPath, string encodedPath)
    {
        using var process = _codec.CreateDecoder(originalPath, encodedPath);

        await _processRunner.RunTimedAsync(process);
        
        if (process.ExitCode == 0)
        {
            return true;
        }
        
        _logger.LogError("Could not decode file {path}. Exit code: {code}.",
            encodedPath, process.ExitCode);

        return false;
    }
}