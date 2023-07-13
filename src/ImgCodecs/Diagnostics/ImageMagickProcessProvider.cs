using System.Diagnostics;
using ImgCodecs.Configuration;
using ImgCodecs.Images;

namespace ImgCodecs.Diagnostics;

public class ImageMagickProcessProvider : ICodecProcessProvider
{
    private const string ProcFilename = "magick";

    private readonly string _targetExtension;
    private readonly ITempDirectoryProvider _tempDirectoryProvider;

    public ImageMagickProcessProvider(BenchmarkType benchmarkType, ITempDirectoryProvider tempDirectoryProvider)
    {
        _targetExtension = GetExtension(benchmarkType);
        _tempDirectoryProvider = tempDirectoryProvider;
    }
    
    public Process CreateForEncoding(string originalFilePath, out string tempEncodedFilePath)
    {
        tempEncodedFilePath = _tempDirectoryProvider.SupplyPathForEncoded(originalFilePath, _targetExtension);

        return CreateCore(originalFilePath, tempEncodedFilePath);
    }
    
    public Process CreateForDecoding(string originalFilePath, string encodedFilePath)
    {
        var tempDecodedFilePath =
            _tempDirectoryProvider.SupplyPathForDecoded(originalFilePath, _targetExtension);

        return CreateCore(encodedFilePath, tempDecodedFilePath);
    }

    private static Process CreateCore(string sourceFile, string targetFile)
    {
        return new()
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = ProcFilename,
                ArgumentList =
                {
                    sourceFile,
                    targetFile
                }
            }
        };
    }

    private static string GetExtension(BenchmarkType benchmarkType)
        => benchmarkType switch
        {
            BenchmarkType.JpegLs => ".ls",
            BenchmarkType.Jpeg2000 => ".jp2",
            BenchmarkType.JpegXl => ".jxl",
            BenchmarkType.Flif => ".flif",
            _ => throw new InvalidOperationException("Invalid benchmark type.")
        };
}