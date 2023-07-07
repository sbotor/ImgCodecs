using CommandLine;
using Microsoft.Extensions.Logging;

namespace ImgCodecs;

[Verb("run", isDefault: true)]
public class BenchmarkingOptions
{
    [Option('c', "count", HelpText = "Run count per image. Default: 1.")]
    public int RunCount { get; set; } = 1;
    
    [Option('w', "warmup-count", HelpText = "Warmup run count per image. Default: 0.")]
    public int WarmupCount { get; set; }
    
    [Option('b', "batch-size", HelpText = "Batch size of images to process before updating the results. Default: 5.")]
    public int BatchSize { get; set; } = 5;

    [Option('t', "timeout", HelpText = "Timeout per one image run in milliseconds. Default: 1000.")]
    public int Timeout { get; set; } = 1000;

    [Option('l', "list", HelpText = "Path to the image list CSV file. Default: './images.csv'.")]
    public string? ListPath { get; set; }

    [Option('i', "images", HelpText = "Path to the directory containing the images. Default: './images/'.")]
    public string? ImageDirectoryPath { get; set; }
    
    [Option("temp", HelpText = "Path to the temp directory used for processing. Default: './temp/'.")]
    public string? TempDirectoryPath { get; set; }

    [Option('r', "results", HelpText = "Path to the results CSV file. Default: './results.csv'.")]
    public string? ResultsPath { get; set; }

    [Option('h', "home", HelpText = "Path used as home for all default paths. Default to current directory.")]
    public string HomePath { get; set; } = ".";

    [Option("log-level", HelpText = "Minimum log level. Default: Information.")]
    public LogLevel MinimumLogLevel { get; set; } = LogLevel.Information;

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
        return string.IsNullOrEmpty(value)
            ? Path.Join(HomePath, defaultValue)
            : value;
    }
}