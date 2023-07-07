using System.Diagnostics;

namespace ImgCodecs.Diagnostics;

public interface ICodecProcessFactory
{
    Process CreateForEncoding(string filePath, out string tempFilePath);
    Process CreateForDecoding(string filePath);
}