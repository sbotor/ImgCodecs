using System.Diagnostics;
using ImgCodecs.Codecs;
using ImgCodecs.Configuration;
using ImgCodecs.Images;
using ImgCodecs.Logging;
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
        var originalInfo = _imageProvider.GetInfoFromFilename(image.Name);
        var results = new BenchmarkImageResults(image, originalInfo.ByteCount);
        
        _logger.LogDebug("Processing image {name}.", image.Name);

        try
        {
            await RunWithCancellation(originalInfo, results, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            return results;
        }

        return results;
    }

    private async Task RunWithCancellation(
        ImageFileInfo originalInfo, BenchmarkImageResults results,
        CancellationToken cancellationToken)
    {
        for (var i = 0; i < _settings.WarmupCount; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var encodedPath = await MeasureEncoding(originalInfo.FullPath, cancellationToken);
            if (encodedPath is null)
            {
                continue;
            }
            
            cancellationToken.ThrowIfCancellationRequested();
            await MeasureDecoding(originalInfo.FullPath, encodedPath, cancellationToken);
        }

        for (var i = 0; i < _settings.RunCount; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var encodedPath = await MeasureEncoding(originalInfo.FullPath, cancellationToken);
            if (encodedPath is null)
            {
                continue;
            }
            
            var encodingTime = _codecRunner.LastRunTime;
            var encodedInfo = _imageProvider.GetInfoFromPath(encodedPath);
            
            cancellationToken.ThrowIfCancellationRequested();
            var decodingSuccess = await MeasureDecoding(
                originalInfo.FullPath,
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

    private async Task<string?> MeasureEncoding(string path, CancellationToken cancellationToken)
    {
        using var encoder = _codec.CreateEncoder(path);

        var success = await _codecRunner.RunTimedAsync(encoder, cancellationToken);

        return success ? encoder.EncodedPath : null;
    }

    private async Task<bool> MeasureDecoding(string originalPath, string encodedPath,
        CancellationToken cancellationToken)
    {
        using var decoder = _codec.CreateDecoder(originalPath, encodedPath);

        return await _codecRunner.RunTimedAsync(decoder, cancellationToken);
    }
}