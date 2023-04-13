namespace GCDService.Helpers;

public static class NullableExt
{
    public static int? ToNullInt(this string? str)
    {
        return int.TryParse(str, out var tmp) ? tmp : null;
    }

    public static long? ToNullLong(this string? str)
    {
        return long.TryParse(str, out var tmp) ? tmp : null;
    }
}