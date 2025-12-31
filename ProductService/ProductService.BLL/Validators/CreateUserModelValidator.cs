using FluentValidation;
using ProductService.BLL.Models.User;

namespace ProductService.BLL.Validators;

public class CreateUserModelValidator : AbstractValidator<CreateUserModel>
{
    public CreateUserModelValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("{PropertyName} is required.")
            .Length(3, 50).WithMessage("{PropertyName} must be between {MinLength} and {MaxLength} characters.")
            .Matches("^[a-zA-Z0-9_.-]+$").WithMessage("{PropertyName} can only contain letters, numbers, underscores, dots, and hyphens.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("{PropertyName} is required.")
            .EmailAddress().WithMessage("Invalid email format.")
            .MaximumLength(100).WithMessage("{PropertyName} must not exceed {MaxLength} characters.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("{PropertyName} is required.")
            .MinimumLength(6).WithMessage("{PropertyName} must be at least {MinLength} characters long.")
            .MaximumLength(50).WithMessage("{PropertyName} must not exceed {MaxLength} characters.")
            .Matches("[A-Z]").WithMessage("{PropertyName} must contain at least one uppercase letter.")
            .Matches("[a-z]").WithMessage("{PropertyName} must contain at least one lowercase letter.")
            .Matches("[0-9]").WithMessage("{PropertyName} must contain at least one number.");
    }
}
