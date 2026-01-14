using ProductService.BLL.Extensions;
using Shouldly;
using Xunit;

namespace ProductService.Tests.Extensions;

public class StringFormatterTests
{
    [Theory]
    [InlineData(null, "")]
    [InlineData("", "")]
    [InlineData("   ", "")]
    public void ToSentenceCase_ShouldReturnEmpty_WhenInputIsNullOrWhiteSpace(string? input, string expected)
    {
        var result = input.ToSentenceCase();

        result.ShouldBe(expected);
    }

    [Theory]
    [InlineData("hello", "Hello")]
    [InlineData("HELLO", "Hello")]
    [InlineData("  hElLo  ", "Hello")]
    [InlineData("a", "A")]
    public void ToSentenceCase_ShouldFormatStringCorrectly(string input, string expected)
    {
        var result = input.ToSentenceCase();

        result.ShouldBe(expected);
    }
}
