using ImgCodecs.Configuration;
using ImgCodecs.Extensions;
using ImgCodecs.Images;
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

    public async Task RunAsync()
    {
        var images = await _imageProvider.ReadListEntriesAsync();

        if (images.Count < 1)
        {
            _logger.LogWarning("No images to process found.");
            return;
        }
        
        if (_settings.ImageBatchSize < 0)
        {
            var results = await _runner.RunBatchAsync(images);
            await _sink.CollectAndWriteAsync(results);
            
            _logger.LogInformation("Finished processing {count} images.", _sink.WrittenCount);
            
            return;
        }

        var batchCount = 0;
        var expectedBatchCount = images.Count.DivideWithCeiling(_settings.ImageBatchSize);
        
        foreach (var batch in images.Chunk(_settings.ImageBatchSize))
        {
            var batchResults = await _runner.RunBatchAsync(batch);
            await _sink.CollectAndWriteAsync(batchResults);

            batchCount++;
            _logger.LogInformation("Finished processing batch {count}/{expCount}.",
                batchCount, expectedBatchCount);
        }
        
        _logger.LogInformation("Finished processing {count} images.", _sink.WrittenCount);
    }
}