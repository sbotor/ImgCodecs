using System.Globalization;
using CsvHelper;
using ImgCodecs.Configuration;
using Microsoft.Extensions.Options;

namespace ImgCodecs.Images;

public interface IImageProvider
{
    ImageFileInfo GetInfoFromFilename(string filename);
    Task<IReadOnlyCollection<ImageEntry>> ReadListEntriesAsync();
    ImageFileInfo GetInfoFromPath(string path);
}

public class ImageProvider : IImageProvider
{
    private readonly DirectorySettings _settings;
    
    public ImageProvider(IOptions<DirectorySettings> options)
    {
        _settings = options.Value;
    }

    public async Task<IReadOnlyCollection<ImageEntry>> ReadListEntriesAsync()
    {
        var list = new List<ImageEntry>();
        
        using var reader = new StreamReader(_settings.ImageListPath);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

        csv.Context.RegisterClassMap<ImageEntry.CsvMap>();
        
        await foreach (var record in csv.GetRecordsAsync<ImageEntry>())
        {
            list.Add(record);
        }

        return list;
    }

    public ImageFileInfo GetInfoFromPath(string path)
    {
        var file = new FileInfo(path);
        var size = file.Exists ? file.Length : -1;
        
        return new(path, size);
    }

    public ImageFileInfo GetInfoFromFilename(string filename)
    {
        var fullPath = Path.Join(_settings.ImagesDirectoryPath, filename);

        return GetInfoFromPath(fullPath);
    }
}