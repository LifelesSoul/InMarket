using FluentValidation;
using ProductService.BLL.Models.Product;
using ProductService.Domain.Constants;

namespace ProductService.BLL.Validators;

public class CreateProductModelValidator : AbstractValidator<CreateProductModel>
{
    public CreateProductModelValidator()
    {
        RuleFor(x => x.Title).ProductTitle();

        RuleFor(x => x.Price).ProductPrice();

        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage(ValidationMessages.Required);

        RuleFor(x => x.ImageUrls)
            .Must(ProductRules.HaveOnlyHttpLinks).WithMessage(ValidationMessages.ImageUrls);
    }
}
