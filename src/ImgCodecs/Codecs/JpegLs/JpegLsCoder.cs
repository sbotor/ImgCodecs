namespace ImgCodecs.Codecs.JpegLs;

public class JpegLsCoder : ICodecEncoder
{
    private readonly JpegLsParameters _parameters;
    private readonly IProcessRunner _processRunner;

    public string EncodedPath => _parameters.DestinationPath;

    public JpegLsCoder(JpegLsParameters parameters, IProcessRunner processRunner)
    {
        _parameters = parameters;
        _processRunner = processRunner;
    }

    public async Task<bool> RunAsync(CancellationToken cancellationToken)
    {
        var scriptFilename = _parameters.Decode
            ? JpegLsHelpers.DecodeFilename
            : JpegLsHelpers.EncodeFilename;
        
        var scriptPath = Path.Join(_parameters.ScriptsPath, scriptFilename);
        var pythonPath = Path.Join(
            _parameters.ScriptsPath,
            "venv", "bin", "python3");
        
        var startInfo = ProcessHelpers.CreateStartInfo(pythonPath, new[]
        {
            scriptPath,
            _parameters.SourcePath,
            _parameters.DestinationPath
        });
        
        return await _processRunner.RunAsync(startInfo, cancellationToken) == 0;
    }
}