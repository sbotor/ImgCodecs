using System.Diagnostics;
using ImgCodecs.Configuration;
using ImgCodecs.Images;

namespace ImgCodecs.Diagnostics;

public class FfmpegCodec : ICodec
{
    private const string ProcFilename = "ffmpeg";
    private const string TargetExtension = ".mp4";
    
    private const string Crf = "28";

    private readonly BenchmarkType _benchmarkType;
    private readonly ITempDirectoryProvider _tempDirectoryProvider;

    public FfmpegCodec(BenchmarkType benchmarkType, ITempDirectoryProvider tempDirectoryProvider)
    {
        _benchmarkType = benchmarkType;
        _tempDirectoryProvider = tempDirectoryProvider;
    }
    
    public EncodingInfo CreateEncoder(string originalFilePath)
    {
        var tempEncodedFilePath = _tempDirectoryProvider.SupplyPathForEncoded(originalFilePath, TargetExtension);
        var process = CreateCore(GetEncoderArgs(originalFilePath, tempEncodedFilePath));

        return new(process, tempEncodedFilePath);
    }

    public Process CreateDecoder(string originalFilePath, string encodedFilePath)
    {
        var tempDecodedFilePath = _tempDirectoryProvider.SupplyPathForDecoded(originalFilePath);

        return CreateCore(GetDecoderArgs(encodedFilePath, tempDecodedFilePath));
    }

    private static Process CreateCore(IEnumerable<string> args)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = ProcFilename
        };

        foreach (var arg in args)
        {
            startInfo.ArgumentList.Add(arg);
        }
        
        return new()
        {
            StartInfo = startInfo
        };
    }

    private IEnumerable<string> GetEncoderArgs(string originalFilePath, string tempEncodedFilePath)
    {
        if (_benchmarkType == BenchmarkType.Hevc)
        {
            return new[]
            {
                "-y",
                "-loop", "1",
                "-i", originalFilePath,
                "-c:v", "libx265",
                "-crf", Crf,
                "-t", "1",
                tempEncodedFilePath
            };
        }
        else
        {
            return new[]
            {
                "-y",
                "-loop", "1",
                "-i", originalFilePath,
                "-c:v", "vvc",
                "-t", "1",
                tempEncodedFilePath
            };
        }
    }

    private static IEnumerable<string> GetDecoderArgs(string encodedFilePath, string tempDecodedFilePath)
        => new[]
        {
            "-y",
            "-i", encodedFilePath,
            "-frames:v", "1",
            tempDecodedFilePath
        };
}