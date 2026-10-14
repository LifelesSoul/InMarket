using FluentValidation;
using ProductService.Domain.Constants;

namespace ProductService.BLL.Validators;

internal static class ProductRules
{
    public static IRuleBuilderOptions<T, string> ProductTitle<T>(this IRuleBuilder<T, string> rule)
    {
        return rule
            .NotEmpty().WithMessage(ValidationMessages.Required)
            .MaximumLength(ValidationConstants.Product.TitleMaxLength)
                .WithMessage(ValidationMessages.MaxLength);
    }

    public static IRuleBuilderOptions<T, decimal> ProductPrice<T>(this IRuleBuilder<T, decimal> rule)
    {
        return rule
            .GreaterThan(0m).WithMessage(ValidationMessages.PositivePrice)
            .PrecisionScale(
                ValidationConstants.Product.PricePrecision,
                ValidationConstants.Product.PriceScale,
                ignoreTrailingZeros: true)
                .WithMessage(ValidationMessages.PriceFormat);
    }

    public static bool HaveOnlyHttpLinks(IEnumerable<string>? urls)
    {
        return urls is null || urls.All(IsHttpLink);
    }

    private static bool IsHttpLink(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var uri)
            && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }
}
