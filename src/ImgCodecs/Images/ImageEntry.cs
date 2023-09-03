namespace ImgCodecs.Images;

public record ImageEntry(string Name, bool IsPhoto, ImageDimensions Dimensions)
{
    public static readonly ImageEntry Empty = new ImageEntry(string.Empty, default, default);
};