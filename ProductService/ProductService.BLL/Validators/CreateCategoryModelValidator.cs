using FluentValidation;
using ProductService.BLL.Models.Category;
using ProductService.DAL.Repositories;

namespace ProductService.BLL.Validators;

public class CreateCategoryModelValidator : AbstractValidator<CreateCategoryModel>
{
    public CreateCategoryModelValidator(ICategoryRepository repository)
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Category name is required.")
            .MustAsync(async (name, cancellation) =>
            {
                return !await repository.IsNameTaken(name.Trim(), excludeId: null, cancellation);
            })
            .WithMessage("Category with this name already exists.");
    }
}
