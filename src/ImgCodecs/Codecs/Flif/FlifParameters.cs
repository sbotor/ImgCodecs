namespace ImgCodecs.Codecs.Flif;

public class FlifParameters : CodecParameters
{
    public bool Decode { get; init; }
    
    public FlifParameters(string sourcePath, string destinationPath)
        : base(sourcePath, destinationPath)
    {
    }
}