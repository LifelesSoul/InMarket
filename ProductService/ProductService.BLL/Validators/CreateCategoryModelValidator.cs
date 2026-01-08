using FluentValidation;
using ProductService.BLL.Models.Category;
using ProductService.Domain.Constants;

namespace ProductService.BLL.Validators;

public class CreateCategoryModelValidator : AbstractValidator<CreateCategoryModel>
{
    public CreateCategoryModelValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("{PropertyName} is required.")
            .MinimumLength(ValidationConstants.Category.NameMinLength)
                .WithMessage("Category name must be at least {MinLength} characters long.")
            .MaximumLength(ValidationConstants.Category.NameMaxLength)
                .WithMessage("Category name must not exceed {MaxLength} characters.");
    }
}
