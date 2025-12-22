using FluentValidation;
using ProductService.BLL.Models.Category;
using ProductService.DAL.Repositories;

namespace ProductService.BLL.Validators;

public class UpdateCategoryModelValidator : AbstractValidator<CategoryModel>
{
    public UpdateCategoryModelValidator(ICategoryRepository repository)
    {
        RuleFor(x => x.Id)
            .NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Category name is required.")
            .MinimumLength(2).WithMessage("Category name must be at least 2 characters long.")
            .MaximumLength(50).WithMessage("Category name must not exceed 50 characters.");
    }
}
