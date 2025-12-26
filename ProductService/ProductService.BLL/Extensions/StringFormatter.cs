namespace ProductService.BLL.Extensions;

public static class StringFormatter
{
    public static string ToSentenceCase(this string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        var trimmed = input.Trim();
        var lowerCased = trimmed.ToLower();
        return char.ToUpper(lowerCased[0]) + lowerCased[1..];
    }
}
