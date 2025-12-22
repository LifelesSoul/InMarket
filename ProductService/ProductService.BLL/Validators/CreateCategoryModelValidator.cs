using FluentValidation;
using ProductService.BLL.Models.Category;

namespace ProductService.BLL.Validators;

public class CreateCategoryModelValidator : AbstractValidator<CreateCategoryModel>
{
    public CreateCategoryModelValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Category name is required.")
            .MinimumLength(2).WithMessage("Category name must be at least 2 characters long.")
            .MaximumLength(50).WithMessage("Category name must not exceed 50 characters.");
    }
}
