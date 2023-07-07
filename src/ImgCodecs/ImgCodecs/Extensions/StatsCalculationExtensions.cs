using ImgCodecs.Benchmarking;

namespace ImgCodecs.Extensions;

public static class StatsCalculationExtensions
{
    public static BenchmarkImageStats CalculateStats(this BenchmarkImageResults results)
    {
        var measurements = results.Measurements;
        
        if (measurements.Count < 1)
        {
            return BenchmarkImageStats.Empty(results.Image);
        }

        var n = measurements.Count;
        var means = new Means();

        foreach (var value in measurements)
        {
            means.Collect(value);
        }

        means.Divide(n);

        var variances = new Variances(means);
        foreach (var value in measurements)
        {
            variances.Collect(value);
        }
        
        variances.Divide(n);

        return new(
            new(means.EncodingTime, variances.EncodingTime),
            new(means.DecodingTime, variances.DecodingTime),
            new(means.EncodedSize, variances.EncodedSize),
            results.OriginalImageSize,
            results.Image,
            measurements.Count);
    }
    
    private ref struct Means
    {
        public double EncodingTime { get; private set; }
        public double DecodingTime { get; private set; }
        public double EncodedSize { get; private set; }

        public void Collect(BenchmarkMeasurement value)
        {
            EncodingTime += value.EncodingTime;
            DecodingTime += value.DecodingTime;
            EncodedSize += value.EncodedSize;
        }

        public void Divide(int count)
        {
            EncodingTime /= count;
            DecodingTime /= count;
            EncodedSize /= count;
        }
    }
    
    private ref struct Variances
    {
        private readonly Means _means;

        public Variances(Means means)
        {
            _means = means;
        }
        
        public double EncodingTime { get; private set; }
        public double DecodingTime { get; private set; }
        public double EncodedSize { get; private set; }

        public void Collect(BenchmarkMeasurement value)
        {
            EncodingTime += Calc(_means.EncodingTime, value.EncodingTime);
            DecodingTime += Calc(_means.DecodingTime, value.DecodingTime);
            EncodedSize += Calc(_means.EncodedSize, value.EncodedSize);
        }

        public void Divide(int count)
        {
            EncodingTime /= count;
            DecodingTime /= count;
            EncodedSize /= count;
        }

        private static double Calc(double mean, double x)
        {
            x -= mean;
            return x * x;
        }
    }
}