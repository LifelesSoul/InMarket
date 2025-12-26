using FluentValidation;
using ProductService.BLL.Models.Category;

namespace ProductService.BLL.Validators;

public class CreateCategoryModelValidator : AbstractValidator<CreateCategoryModel>
{
    public CreateCategoryModelValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("{PropertyName} is required.")
            .MinimumLength(2).WithMessage("Category name must be at least {MinLength} characters long.")
            .MaximumLength(50).WithMessage("Category name must not exceed {MaxLength} characters.");
    }
}
