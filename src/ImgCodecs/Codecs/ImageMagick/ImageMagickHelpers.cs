using ImgCodecs.Configuration;

namespace ImgCodecs.Codecs.ImageMagick;

public static class ImageMagickHelpers
{
    public const string MagickFilename = "convert";
    
    public static string GetExtensionAndProcFilename(BenchmarkType benchmarkType)
        => benchmarkType switch
        {
            BenchmarkType.JpegLs => ".ls",
            BenchmarkType.Jpeg2000 => ".jp2",
            BenchmarkType.Flif => ".flif",
            _ => throw new InvalidOperationException("Invalid benchmark type.")
        };
}