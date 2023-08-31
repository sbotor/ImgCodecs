using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using ImgCodecs.Configuration;
using ImgCodecs.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ImgCodecs.Benchmarking;

public interface IResultsSink
{
    Task CollectAndWriteAsync(IEnumerable<BenchmarkImageResults> results);
    int WrittenCount { get; }
}

public class CsvFileResultsSink : IResultsSink
{
    private readonly ILogger<CsvFileResultsSink> _logger;
    private readonly DirectorySettings _settings;

    public int WrittenCount { get; private set; }

    public CsvFileResultsSink(IOptions<DirectorySettings> options, ILogger<CsvFileResultsSink> logger)
    {
        _settings = options.Value;
        _logger = logger;
    }
    
    public async Task CollectAndWriteAsync(IEnumerable<BenchmarkImageResults> results)
    {
        _logger.LogDebug("Writing results.");

        await using var csv = CreateWriter();

        var records = CalculateStats(results);
        await csv.WriteRecordsAsync(records);

        WrittenCount += records.Count;
        
        _logger.LogDebug("Written {count} results. {total} total.",
            records.Count, WrittenCount);
    }

    private static IReadOnlyCollection<BenchmarkImageStats> CalculateStats(IEnumerable<BenchmarkImageResults> results)
        => results.Select(x => x.CalculateStats()).ToArray();

    private CsvWriter CreateWriter()
    {
        var append = WrittenCount > 0;
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = !append
        };
        
        CsvWriter? writer = null;
        try
        {
            writer = new CsvWriter(new StreamWriter(_settings.ResultsPath, append), config);
            writer.Context.RegisterClassMap<ResultsMap>();
            
            return writer;
        }
        catch
        {
            writer?.Dispose();
            throw;
        }
    }
    
    private sealed class ResultsMap : ClassMap<BenchmarkImageStats>
    {
        public ResultsMap()
        {
            Map(x => x.Image.Name)
                .Name("name");
            Map(x => x.Image.IsPhoto)
                .Name("photo");

            Map(x => x.EncodingTime.Mean)
                .Name("encoding_time_mean");
            Map(x => x.EncodingTime.Variance)
                .Name("encoding_time_var");
            
            Map(x => x.DecodingTime.Mean)
                .Name("decoding_time_mean");
            Map(x => x.DecodingTime.Variance)
                .Name("decoding_time_var");
            
            Map(x => x.OriginalSize)
                .Name("original_size");
            Map(x => x.CompressionRatio)
                .Name("compression_ratio");
            
            Map(x => x.EncodedSize.Mean)
                .Name("encoded_size_mean");
            Map(x => x.EncodedSize.Variance)
                .Name("encoded_size_var");

            Map(x => x.MeasurementCount)
                .Name("measurement_count");
        }
    }
}