using FluentValidation;
using ProductService.BLL.Models.Product;
using ProductService.Domain.Constants;

namespace ProductService.BLL.Validators;

public class UpdateProductModelValidator : AbstractValidator<UpdateProductModel>
{
    public UpdateProductModelValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage(ValidationMessages.Required);

        RuleFor(x => x.Title).ProductTitle();

        RuleFor(x => x.Price).ProductPrice();

        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage(ValidationMessages.Required);

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage(ValidationMessages.InvalidStatus);

        RuleFor(x => x.ImageUrls)
            .Must(ProductRules.HaveOnlyHttpLinks).WithMessage(ValidationMessages.ImageUrls);
    }
}
