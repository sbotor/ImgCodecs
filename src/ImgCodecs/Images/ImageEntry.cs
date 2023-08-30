using CsvHelper.Configuration;

namespace ImgCodecs.Images;

public class ImageEntry
{
    public string Name { get; set; } = string.Empty;
    public bool IsPhoto { get; set; }
    
    public class CsvMap : ClassMap<ImageEntry>
    {
        public CsvMap()
        {
            Map(x => x.Name).Name("name");
            Map(x => x.IsPhoto).Name("photo");
        }
    }
};