using System.Runtime.InteropServices;
using ImgCodecs.Configuration;

namespace ImgCodecs.Codecs.ImageMagick;

public static class ImageMagickHelpers
{
    public static readonly string ProcFilename;
    
    static ImageMagickHelpers()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            ProcFilename = "magick";
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            ProcFilename = "convert";
        }
        else
        {
            throw new PlatformNotSupportedException();
        }
    }
    
    public static string GetExtension(BenchmarkType benchmarkType)
        => benchmarkType switch
        {
            BenchmarkType.JpegLs => ".ls",
            BenchmarkType.Jpeg2000 => ".jp2",
            BenchmarkType.JpegXl => ".jxl",
            BenchmarkType.Flif => ".flif",
            _ => throw new InvalidOperationException("Invalid benchmark type.")
        };
}