using CommandLine;
using ImgCodecs.Configuration;
using Microsoft.Extensions.Logging;

namespace ImgCodecs;

[Verb("run", isDefault: true)]
public class BenchmarkingOptions
{
    [Value(0, HelpText = "Benchmark type to run. Allowed values: JpegLs, Jpeg2000, JpegXl, Flif, Hevc, Vvc.")]
    public BenchmarkType BenchmarkType { get; set; }
    
    [Option('c', "count", HelpText = "Run count per image. Default: 1.")]
    public int RunCount { get; set; } = 1;
    
    [Option('w', "warmup-count", HelpText = "Warmup run count per image. Default: 0.")]
    public int WarmupCount { get; set; }
    
    [Option('b', "batch-size", HelpText = "Batch size of images to process before updating the results. Default: 5.")]
    public int BatchSize { get; set; } = 5;

    [Option('t', "timeout", HelpText = "Timeout per one image run in milliseconds. Default: 1000.")]
    public int Timeout { get; set; } = 1000;

    [Option('l', "list", HelpText = "Path to the image list CSV file. Default: '{root}/images.csv'.")]
    public string? ListPath { get; set; }

    [Option('i', "images", HelpText = "Path to the directory containing the images. Default: '{root}/images/'.")]
    public string? ImageDirectoryPath { get; set; }
    
    [Option("temp", HelpText = "Path to the temp directory used for processing. Default: '{root}/temp/'.")]
    public string? TempDirectoryPath { get; set; }

    [Option('o', "output", HelpText = "Path to the results CSV file. Default: '{root}/results.csv'.")]
    public string? ResultsPath { get; set; }

    [Option('r', "root", HelpText = "Path used as root for all other paths. Defaults to current directory.")]
    public string HomePath { get; set; } = ".";
    
    [Option("temp-cleanup", HelpText = "Temporary files cleanup behavior. Allowed values: DeleteAll, PreserveEncoded, PreserveDecoded, PreserveAll.")]
    public TempCleanupBehavior TempCleanupBehavior { get; set; }

    [Option('t', "threads", HelpText = "Thread count. Defaults to max virtual threads for the current processor.")]
    public int Threads { get; set; }

    public string GetEffectiveListPath()
        => GetEffectivePath(ListPath, "images.csv");
    
    public string GetEffectiveImageDirPath()
        => GetEffectivePath(ImageDirectoryPath, "images");
    
    public string GetEffectiveTempDirPath()
        => GetEffectivePath(TempDirectoryPath, "temp");
    
    public string GetEffectiveResultsPath()
        => GetEffectivePath(ResultsPath, "results.csv");
    
    private string GetEffectivePath(string? value, string defaultValue)
    {
        if (string.IsNullOrEmpty(value))
        {
            value = defaultValue;
        }
        
        return Path.Join(HomePath, value);
    }
}