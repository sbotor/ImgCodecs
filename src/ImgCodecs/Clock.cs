namespace ImgCodecs;

public interface IClock
{
    public DateTime Now { get; }
}

public class Clock : IClock
{
    public DateTime Now => DateTime.Now;
}