using FluentValidation;
using ProductService.BLL.Models.Category;
using ProductService.Domain.Constants;

namespace ProductService.BLL.Validators;

public class CreateCategoryModelValidator : AbstractValidator<CreateCategoryModel>
{
    public CreateCategoryModelValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(ValidationMessages.Required)
            .MinimumLength(ValidationConstants.Category.NameMinLength)
                .WithMessage(ValidationMessages.MinLength)
            .MaximumLength(ValidationConstants.Category.NameMaxLength)
                .WithMessage(ValidationMessages.MaxLength);
    }
}
