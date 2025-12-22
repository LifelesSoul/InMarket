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
            .MustAsync(async (model, name, cancellation) =>
            {
                return !await repository.IsNameTaken(name.Trim(), excludeId: model.Id, cancellation);
            })
            .WithMessage("Category with this name already exists.");
    }
}
