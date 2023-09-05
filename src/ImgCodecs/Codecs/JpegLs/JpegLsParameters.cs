namespace ImgCodecs.Codecs.JpegLs;

public class JpegLsParameters : CodecParameters
{
    public string ScriptsPath { get; }
    public bool Decode { get; init; }

    public JpegLsParameters(string sourcePath, string destinationPath,
        string scriptsPath)
        : base(sourcePath, destinationPath)
    {
        ScriptsPath = scriptsPath;
    }
}