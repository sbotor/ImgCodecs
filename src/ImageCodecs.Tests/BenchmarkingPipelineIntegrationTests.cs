using System.Diagnostics;
using ImgCodecs.Benchmarking;
using ImgCodecs.Codecs;
using ImgCodecs.Configuration;
using ImgCodecs.Images;
using ImgCodecs.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;

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
        tempDirProvider.SupplyPathForDecoded(Arg.Any<string>())
            .Returns("test");

        var processRunner = Substitute.For<IProcessRunner>();
        processRunner.RunAsync(Arg.Any<ProcessStartInfo>(), Arg.Any<CancellationToken>())
            .Returns(0);
        
        var codecFactory = new CodecFactory(tempDirProvider, processRunner);

        var codecRunner = Substitute.For<ICodecRunner>();
        codecRunner.LastRunTime.Returns(_ => Random.Shared.NextDouble());
        codecRunner.RunTimedAsync(Arg.Any<ICodecCoder>(), Arg.Any<CancellationToken>())
            .Returns(true);

        var runner = new BenchmarkRunner(
            benchmarkOptions,
            codecFactory,
            imgProvider,
            codecRunner,
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
        
        await _sut.RunAsync(CancellationToken.None);
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