namespace ImgCodecs.Codecs.Flif;

public sealed class FlifCoder : ICodecEncoder
{
    private readonly FlifParameters _parameters;
    private readonly IProcessRunner _processRunner;

    public string EncodedPath => _parameters.DestinationPath;

    public FlifCoder(FlifParameters parameters, IProcessRunner processRunner)
    {
        _parameters = parameters;
        _processRunner = processRunner;
    }
    
    public async Task<bool> RunAsync(CancellationToken cancellationToken)
    {
        var decodeSwitch = _parameters.Decode ? "-d" : "-e";
        
        var startInfo = ProcessHelpers.CreateStartInfo(FlifHelpers.Filename, new[]
        {
            decodeSwitch,
            "--overwrite",
            "-q", "100",
            _parameters.SourcePath,
            _parameters.DestinationPath
        });

        return await _processRunner.RunAsync(startInfo, cancellationToken) == 0;
    }
}
