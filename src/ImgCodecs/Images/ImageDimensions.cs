namespace ImgCodecs.Images;

public readonly struct ImageDimensions
{
    public int Width { get; }
    public int Height { get; }

    public ImageDimensions(int width, int height)
    {
        Width = width;
        Height = height;
    }

    public override string ToString()
        => $"{Width}x{Height}";
}