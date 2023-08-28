using System.Diagnostics;
using ImgCodecs.Benchmarking;
using ImgCodecs.Configuration;
using ImgCodecs.Diagnostics;
using ImgCodecs.Images;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using NSubstitute.Core;

namespace ImageCodecs.Tests;

public class BenchmarkingPipelineIntegrationTests
{
    private readonly BenchmarkSettings _benchmarkSettings;
    private readonly BenchmarkingPipeline _sut;

    public BenchmarkingPipelineIntegrationTests()
    {
        _benchmarkSettings = new()
        {
            RunCount = 10,
            WarmupCount = 5,
            ImageBatchSize = 5
        };
        
        var benchmarkOptions = Substitute.For<IOptions<BenchmarkSettings>>();
        benchmarkOptions.Value.Returns(_benchmarkSettings);

        var imgProvider = Substitute.For<IImageProvider>();
        imgProvider.GetInfoFromFilename(Arg.Any<string>())
            .Returns(new ImageFileInfo("img", 10));
        imgProvider.GetInfoFromPath(Arg.Any<string>())
            .Returns(new ImageFileInfo("img", 10));

        var tempDirProvider = Substitute.For<ITempDirectoryProvider>();
        tempDirProvider.SupplyPathForEncoded(Arg.Any<string>(), Arg.Any<string>())
            .Returns("test");
        tempDirProvider.SupplyPathForDecoded(Arg.Any<string>(), Arg.Any<string>())
            .Returns("test");

        var codecFactory = new CodecFactory(tempDirProvider);

        var processRunner = Substitute.For<IProcessRunner>();
        processRunner.LastRunTime.Returns(_ => Random.Shared.NextDouble());
        processRunner.RunTimedAsync(Arg.Any<Process>()).Returns(Task.CompletedTask);

        var runner = new BenchmarkRunner(
            benchmarkOptions,
            codecFactory,
            imgProvider,
            processRunner,
            NullLogger<BenchmarkRunner>.Instance);

        var resultsSink = Substitute.For<IResultsSink>();

        _sut = new(
            benchmarkOptions,
            imgProvider,
            runner,
            resultsSink,
            NullLogger<BenchmarkingPipeline>.Instance);
    }
    
    [Theory]
    [MemberData(nameof(BenchmarkTypes))]
    public async Task Run_ForSuccessfulProcesses_RunsSuccessfully(BenchmarkType type)
    {
        _benchmarkSettings.BenchmarkType = type;
        
        await _sut.RunAsync();
    }

    public static TheoryData<BenchmarkType> BenchmarkTypes()
    {
        var data = new TheoryData<BenchmarkType>();

        foreach (var type in Enum.GetValues<BenchmarkType>())
        {
            data.Add(type);
        }

        return data;
    }
}