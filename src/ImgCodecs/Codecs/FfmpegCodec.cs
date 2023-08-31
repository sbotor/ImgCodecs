using System.Diagnostics;
using ImgCodecs.Configuration;
using ImgCodecs.Images;

namespace ImgCodecs.Codecs;

public class FfmpegCodec : ICodec
{
    private const string ProcFilename = "ffmpeg";
    private const string TargetExtension = ".mp4";

    private readonly BenchmarkType _benchmarkType;
    private readonly string _threads;
    private readonly ITempDirectoryProvider _tempDirectoryProvider;

    public FfmpegCodec(
        BenchmarkType benchmarkType,
        int threads,
        ITempDirectoryProvider tempDirectoryProvider)
    {
        _benchmarkType = benchmarkType;
        _threads = threads.ToString();
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
            FileName = ProcFilename,
            RedirectStandardOutput = true,
            RedirectStandardError = true
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
                "-i", originalFilePath,
                "-c:v", "libx265",
                "-x265-params", "lossless=1",
                "-frames:v", "1",
                "-threads", _threads,
                tempEncodedFilePath
            };
        }
        else
        {
            return new[]
            {
                "-y",
                "-i", originalFilePath,
                "-c:v", "vvc",
                "-frames:v", "1",
                "-threads", _threads,
                tempEncodedFilePath
            };
        }
    }

    private IEnumerable<string> GetDecoderArgs(string encodedFilePath, string tempDecodedFilePath)
        => new[]
        {
            "-y",
            "-i", encodedFilePath,
            "-frames:v", "1",
            "-threads", _threads,
            tempDecodedFilePath
        };
}