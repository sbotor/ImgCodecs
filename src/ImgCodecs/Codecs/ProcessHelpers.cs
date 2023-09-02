using System.Diagnostics;

namespace ImgCodecs.Codecs;

public static class ProcessHelpers
{
    public static ProcessStartInfo CreateStartInfo(string filename, IEnumerable<string> arguments)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = filename,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };
        
        foreach (var arg in arguments)
        {
            startInfo.ArgumentList.Add(arg);
        }

        return startInfo;
    }
}