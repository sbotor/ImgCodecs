using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
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

        csv.Context.RegisterClassMap<CsvImageEntry.CsvMap>();
        
        await foreach (var record in csv.GetRecordsAsync<CsvImageEntry>())
        {
            list.Add(record.ToImageEntry());
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

    private class CsvImageEntry
    {
        public string Name { get; set; } = null!;
        public bool IsPhoto { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public ImageEntry ToImageEntry()
            => new(Name, IsPhoto, new(Width, Height));
        
        public class CsvMap : ClassMap<CsvImageEntry>
        {
            public CsvMap()
            {
                Map(x => x.Name).Name("name");
                Map(x => x.IsPhoto).Name("photo");
                Map(x => x.Width).Name("width");
                Map(x => x.Height).Name("height");
            }
        }
    }
}