using FluentValidation;
using ProductService.BLL.Models.Category;
using ProductService.DAL.Repositories;
using ProductService.Domain.Constants;
using System.Diagnostics.CodeAnalysis;

namespace ProductService.BLL.Validators;

[ExcludeFromCodeCoverage]
public class UpdateCategoryModelValidator : AbstractValidator<CategoryModel>
{
    public UpdateCategoryModelValidator(ICategoryRepository repository)
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage(ValidationMessages.Required);

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(ValidationMessages.Required)
            .MinimumLength(ValidationConstants.Category.NameMinLength)
                .WithMessage(ValidationMessages.MinLength)
            .MaximumLength(ValidationConstants.Category.NameMaxLength)
                .WithMessage(ValidationMessages.MaxLength);
    }
}
