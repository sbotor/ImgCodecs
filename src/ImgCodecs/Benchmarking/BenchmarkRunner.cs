using ImgCodecs.Codecs;
using ImgCodecs.Configuration;
using ImgCodecs.Images;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ImgCodecs.Benchmarking;

public interface IBenchmarkRunner
{
    Task<IReadOnlyCollection<BenchmarkImageResults>> RunBatchAsync(IReadOnlyCollection<ImageEntry> images,
        CancellationToken cancellationToken);
}

public class BenchmarkRunner : IBenchmarkRunner
{
    private readonly BenchmarkSettings _settings;
    private readonly ICodec _codec;
    private readonly IImageProvider _imageProvider;

    private readonly ICodecRunner _codecRunner;
    private readonly ILogger<BenchmarkRunner> _logger;

    public BenchmarkRunner(IOptions<BenchmarkSettings> options,
        ICodecFactory codecFactory,
        IImageProvider imageProvider,
        ICodecRunner codecRunner,
        ILogger<BenchmarkRunner> logger)
    {
        _settings = options.Value;
        _codec = codecFactory.CreateCodec(_settings.BenchmarkType, _settings.Threads);
        _imageProvider = imageProvider;
        _codecRunner = codecRunner;
        _logger = logger;
    }
    
    public async Task<IReadOnlyCollection<BenchmarkImageResults>> RunBatchAsync(IReadOnlyCollection<ImageEntry> images,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug("Started processing a batch of {count} images.", images.Count);
        
        var list = new List<BenchmarkImageResults>(images.Count);
        foreach (var entry in images)
        {
            var result = await RunForImageAsync(entry, cancellationToken);
            list.Add(result);
            
            _logger.LogDebug("Processed image {name}.", entry.Name);

            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }
        }

        return list;
    }

    private async Task<BenchmarkImageResults> RunForImageAsync(ImageEntry image,
        CancellationToken cancellationToken)
    {
        var originalFileInfo = _imageProvider.GetInfoFromFilename(image.Name);
        var results = new BenchmarkImageResults(image, originalFileInfo.ByteCount);
        
        _logger.LogDebug("Processing image {name}.", image.Name);

        try
        {
            var info = new ImageInfo(image, originalFileInfo);
            await RunWithCancellation(info, results, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            return results;
        }

        return results;
    }

    private async Task RunWithCancellation(
        ImageInfo originalInfo, BenchmarkImageResults results,
        CancellationToken cancellationToken)
    {
        for (var i = 0; i < _settings.WarmupCount; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var encodedPath = await MeasureEncoding(originalInfo, cancellationToken);
            if (encodedPath is null)
            {
                continue;
            }
            
            cancellationToken.ThrowIfCancellationRequested();
            await MeasureDecoding(originalInfo, encodedPath, cancellationToken);
        }

        for (var i = 0; i < _settings.RunCount; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var encodedPath = await MeasureEncoding(originalInfo, cancellationToken);
            if (encodedPath is null)
            {
                continue;
            }
            
            var encodingTime = _codecRunner.LastRunTime;
            var encodedInfo = _imageProvider.GetInfoFromPath(encodedPath);
            
            cancellationToken.ThrowIfCancellationRequested();
            var decodingSuccess = await MeasureDecoding(
                originalInfo,
                encodedPath,
                cancellationToken);
            if (!decodingSuccess)
            {
                continue;
            }
            
            var decodingTime = _codecRunner.LastRunTime;
            
            results.Collect(new(
                encodingTime,
                decodingTime,
                encodedInfo.ByteCount));
        }
    }

    private async Task<string?> MeasureEncoding(ImageInfo info, CancellationToken cancellationToken)
    {
        using var encoder = _codec.CreateEncoder(info);

        var success = await _codecRunner.RunTimedAsync(encoder, cancellationToken);

        if (!success)
        {
            _logger.LogWarning("Could not encode file {path}.", info.File.FullPath);
        }

        return success ? encoder.EncodedPath : null;
    }

    private async Task<bool> MeasureDecoding(ImageInfo info, string encodedPath,
        CancellationToken cancellationToken)
    {
        using var decoder = _codec.CreateDecoder(info, encodedPath);

        var success = await _codecRunner.RunTimedAsync(decoder, cancellationToken);

        if (!success)
        {
            _logger.LogWarning("Could not decode file {path}.", encodedPath);
        }
        
        return success;
    }
}