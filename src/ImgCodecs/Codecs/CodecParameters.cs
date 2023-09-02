namespace ImgCodecs.Codecs;

public class CodecParameters
{
    public string SourcePath { get; }
    public string DestinationPath { get; }
    
    public CodecParameters(string sourcePath, string destinationPath)
    {
        SourcePath = sourcePath;
        DestinationPath = destinationPath;
    }
}