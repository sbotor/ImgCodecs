using System.Diagnostics;

namespace ImgCodecs.Diagnostics;

public interface ICodecProcessProvider
{
    Process CreateForEncoding(string originalFilePath, out string tempEncodedFilePath);
    Process CreateForDecoding(string originalFilePath, string encodedFilePath);
}