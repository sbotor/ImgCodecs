using System.Diagnostics;
using ImgCodecs.Benchmarking;
using ImgCodecs.Configuration;
using ImgCodecs.Images;

namespace ImgCodecs.Diagnostics;

public class ImageMagickProcessFactory : ICodecProcessFactory
{
    private const string ProcFilename = "magick";
    
    // TODO
    private readonly BenchmarkType _benchmarkType;
    private readonly ITempFileProvider _tempFileProvider;

    public ImageMagickProcessFactory(BenchmarkType benchmarkType, ITempFileProvider tempFileProvider)
    {
        _benchmarkType = benchmarkType;
        _tempFileProvider = tempFileProvider;
    }
    
    public Process CreateForEncoding(string filePath, out string tempFilePath)
    {
        _tempFileProvider.EnsureDirectory();
        
        tempFilePath = GetTempPathForEncode(filePath);

        return CreateCore($"\"{filePath}\" \"{tempFilePath}\"", tempFilePath);
    }
    
    public Process CreateForDecoding(string filePath)
    {
        _tempFileProvider.EnsureDirectory();
        
        var tempFilePath = GetTempPathForDecode(filePath);

        return CreateCore($"\"{filePath}\" \"{tempFilePath}\"", tempFilePath);
    }

    private Process CreateCore(string arguments, string tempFilename)
    {
        _tempFileProvider.RegisterForDeletion(tempFilename);
        
        return new()
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = ProcFilename,
                Arguments = arguments
            }
        };
    }

    private string GetTempPathForEncode(string originalPath)
    {
        var extension = FileExtensionHelpers.GetExtensionForBenchmark(_benchmarkType);
        var filename = $"E_{Path.GetFileName(originalPath)}{extension}";

        return Path.Combine(_tempFileProvider.TempDirectory, filename);
    }
    
    private string GetTempPathForDecode(string encodedPath)
    {
        var filename = $"D_{Path.GetFileNameWithoutExtension(encodedPath)}";

        return Path.Combine(_tempFileProvider.TempDirectory, filename);
    }
}