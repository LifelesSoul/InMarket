namespace ProductService.BLL.Extensions;

public static class StringFormatter
{
    public static string ToSentenceCase(this string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return string.Empty;
        }

        var words = input.Split(new[] { ' ', ',', '.', ':' }, StringSplitOptions.RemoveEmptyEntries);

        if (words.Length == 0)
        {
            return string.Empty;
        }

        var sentence = string.Join(" ", words).ToLower();

        return char.ToUpper(sentence[0]) + sentence[1..];
    }
}
