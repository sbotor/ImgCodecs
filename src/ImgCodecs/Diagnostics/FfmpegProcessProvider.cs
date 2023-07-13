using System.Diagnostics;
using ImgCodecs.Configuration;
using ImgCodecs.Images;

namespace ImgCodecs.Diagnostics;

public class FfmpegProcessProvider : ICodecProcessProvider
{
    private const string ProcFilename = "ffmpeg";
    
    private const string Crf = "28";
    
    private readonly ITempDirectoryProvider _tempDirectoryProvider;
    private readonly string _codecName;
    private readonly string _targetExtension;

    public FfmpegProcessProvider(BenchmarkType benchmarkType, ITempDirectoryProvider tempDirectoryProvider)
    {
        (_codecName, _targetExtension) = GetCodecInfo(benchmarkType);
        _tempDirectoryProvider = tempDirectoryProvider;
    }
    
    public Process CreateForEncoding(string originalFilePath, out string tempEncodedFilePath)
    {
        tempEncodedFilePath = _tempDirectoryProvider.SupplyPathForEncoded(originalFilePath, _targetExtension);
        
        return new()
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = ProcFilename,
                ArgumentList =
                {
                    "-y",
                    "-loop", "1",
                    "-i", originalFilePath,
                    "-c:v", _codecName,
                    "-crf", Crf,
                    "-t", "1",
                    tempEncodedFilePath
                }
            }
        };
    }

    public Process CreateForDecoding(string originalFilePath, string encodedFilePath)
    {
        var tempDecodedFilePath = _tempDirectoryProvider.SupplyPathForDecoded(originalFilePath, _targetExtension);
        
        return new()
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = ProcFilename,
                ArgumentList =
                {
                    "-y",
                    "-i", encodedFilePath,
                    "-vframes", "1",
                    tempDecodedFilePath
                }
            }
        };
    }

    private static (string CodecName, string Extension) GetCodecInfo(BenchmarkType benchmarkType)
        => benchmarkType switch
        {
            BenchmarkType.Hevc => ("libx265", ".mp4"),
            BenchmarkType.Vvc => ("libvvc", ".mp4"),
            _ => throw new InvalidOperationException("Invalid benchmark type.")
        };
}