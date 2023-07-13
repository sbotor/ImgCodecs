namespace ImgCodecs.Extensions;

public static class NumberExtensions
{
    public static int DivideWithCeiling(this int left, int right)
    {
        var result = left / right;

        if (left % right > 0)
        {
            result++;
        }

        return result;
    }
}