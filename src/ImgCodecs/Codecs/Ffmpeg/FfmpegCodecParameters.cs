namespace ImgCodecs.Codecs.Ffmpeg;

public class FfmpegCodecParameters: CodecParameters
{
    public int Threads { get; }
    
    public FfmpegCodecParameters(string sourcePath, string destinationPath, int threads)
        : base(sourcePath, destinationPath)
    {
        Threads = threads;
    }
}