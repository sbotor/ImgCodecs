using ImgCodecs.Images;

namespace ImgCodecs.Benchmarking;

public class BenchmarkImageResults
{
    private readonly List<BenchmarkMeasurement> _measurements = new();

    public long OriginalImageSize { get; }
    public ImageEntry Image { get; }
    public IReadOnlyCollection<BenchmarkMeasurement> Measurements => _measurements;

    public BenchmarkImageResults(ImageEntry image, long originalSize)
    {
        Image = image;
        OriginalImageSize = originalSize;
    }
    
    public void Collect(BenchmarkMeasurement measurement)
        => _measurements.Add(measurement);
}

public record BenchmarkMeasurement(
    double EncodingTime,
    double DecodingTime,
    long EncodedSize);

public record BenchmarkStats(
    double Mean,
    double Variance)
{
    public static readonly BenchmarkStats Empty = new(default, default);
}

public record BenchmarkImageStats(
    BenchmarkStats EncodingTime,
    BenchmarkStats DecodingTime,
    BenchmarkStats EncodedSize, 
    long OriginalSize,
    ImageEntry Image,
    int MeasurementCount)
{
    public double CompressionRatio { get; } = EncodedSize.Mean > 0
        ? OriginalSize / EncodedSize.Mean
        : 0;
        
    public static BenchmarkImageStats Empty(ImageEntry? entry = null)
        => new(
            BenchmarkStats.Empty,
            BenchmarkStats.Empty,
            BenchmarkStats.Empty,
            default,
            entry ?? new(),
            0);
}
