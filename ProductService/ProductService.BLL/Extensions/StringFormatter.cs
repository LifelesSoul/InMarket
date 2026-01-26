namespace ProductService.BLL.Extensions;

public static class StringFormatter
{
    private static readonly char[] _separators = { ' ', ',', '.', ':' };

    public static string ToSentenceCase(this string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return string.Empty;
        }

        var words = input.Split(_separators, StringSplitOptions.RemoveEmptyEntries);

        if (words.Length == 0)
        {
            return string.Empty;
        }

        var sentence = string.Join(" ", words).ToLower();

        return char.ToUpper(sentence[0]) + sentence[1..];
    }
}
