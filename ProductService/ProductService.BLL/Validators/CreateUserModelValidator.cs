using FluentValidation;
using ProductService.BLL.Models.User;
using ProductService.Domain.Constants;

namespace ProductService.BLL.Validators;

public class CreateUserModelValidator : AbstractValidator<CreateUserModel>
{
    public CreateUserModelValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("{PropertyName} is required.")
            .Length(ValidationConstants.User.UsernameMinLength, ValidationConstants.User.UsernameMaxLength)
                .WithMessage("{PropertyName} must be between {MinLength} and {MaxLength} characters.")
            .Matches(ValidationConstants.User.UsernameRegex)
                .WithMessage("{PropertyName} can only contain letters, numbers, underscores, dots, and hyphens.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("{PropertyName} is required.")
            .EmailAddress().WithMessage("Invalid email format.")
            .MaximumLength(ValidationConstants.User.EmailMaxLength)
                .WithMessage("{PropertyName} must not exceed {MaxLength} characters.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("{PropertyName} is required.")
            .MinimumLength(ValidationConstants.User.PasswordMinLength)
                .WithMessage("{PropertyName} must be at least {MinLength} characters long.")
            .MaximumLength(ValidationConstants.User.PasswordMaxLength)
                .WithMessage("{PropertyName} must not exceed {MaxLength} characters.")
            .Matches(ValidationConstants.User.PasswordUpperCaseRegex)
                .WithMessage("{PropertyName} must contain at least one uppercase letter.")
            .Matches(ValidationConstants.User.PasswordLowerCaseRegex)
                .WithMessage("{PropertyName} must contain at least one lowercase letter.")
            .Matches(ValidationConstants.User.PasswordDigitRegex)
                .WithMessage("{PropertyName} must contain at least one number.");
    }
}
