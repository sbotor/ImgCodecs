using ImgCodecs.Configuration;
using ImgCodecs.Extensions;
using ImgCodecs.Images;
using ImgCodecs.Logging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ImgCodecs.Benchmarking;

public class BenchmarkingPipeline
{
    private readonly BenchmarkSettings _settings;
    private readonly IImageProvider _imageProvider;
    private readonly IBenchmarkRunner _runner;
    private readonly IResultsSink _sink;
    private readonly ILogger<BenchmarkingPipeline> _logger;

    public BenchmarkingPipeline(
        IOptions<BenchmarkSettings> options,
        IImageProvider imageProvider,
        IBenchmarkRunner runner,
        IResultsSink sink,
        ILogger<BenchmarkingPipeline> logger)
    {
        _settings = options.Value;
        _imageProvider = imageProvider;
        _runner = runner;
        _sink = sink;
        _logger = logger;
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        var images = await _imageProvider.ReadListEntriesAsync();
        
        _logger.LogInformation("Starting benchmarks for {type}. Image count: {count}.",
            _settings.BenchmarkType, images.Count);

        if (images.Count < 1)
        {
            _logger.LogWarning("No images to process found.");
            return;
        }

        var batchSize = _settings.ImageBatchSize < 0 ? images.Count : _settings.ImageBatchSize;

        var batchCount = 0;
        var expectedBatchCount = images.Count.DivideWithCeiling(_settings.ImageBatchSize);
        
        foreach (var batch in images.Chunk(batchSize))
        {
            var batchResults = await _runner.RunBatchAsync(batch, cancellationToken);
            await _sink.CollectAndWriteAsync(batchResults);

            batchCount++;
            _logger.LogInformation("Finished processing batch {batchCount}/{expBatchCount} ({count}/{expCount}).",
                batchCount, expectedBatchCount, batchResults.Count, batch.Length);

            cancellationToken.ThrowIfCancellationRequested();
        }
        
        _logger.LogInformation("Finished processing {count} images.", _sink.WrittenCount);
    }
}