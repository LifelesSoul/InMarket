using FluentValidation;
using ProductService.BLL.Models.User;
using ProductService.Domain.Constants;

namespace ProductService.BLL.Validators;

public class UpdateUserModelValidator : AbstractValidator<UpdateUserModel>
{
    public UpdateUserModelValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage(ValidationMessages.Required);

        RuleFor(x => x.Username)
            .NotEmpty().WithMessage(ValidationMessages.Required)
            .Length(ValidationConstants.User.UsernameMinLength, ValidationConstants.User.UsernameMaxLength)
                .WithMessage(ValidationMessages.LengthRange)
            .Matches(ValidationConstants.User.UsernameRegex)
                .WithMessage(ValidationMessages.UsernameRegex);

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage(ValidationMessages.Required)
            .EmailAddress().WithMessage(ValidationMessages.InvalidEmail)
            .MaximumLength(ValidationConstants.User.EmailMaxLength)
                .WithMessage(ValidationMessages.MaxLength);

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage(ValidationMessages.Required)
            .MinimumLength(ValidationConstants.User.PasswordMinLength)
                .WithMessage(ValidationMessages.MinLength)
            .MaximumLength(ValidationConstants.User.PasswordMaxLength)
                .WithMessage(ValidationMessages.MaxLength)
            .Matches(ValidationConstants.User.PasswordUpperCaseRegex)
                .WithMessage(ValidationMessages.PasswordUpperCase)
            .Matches(ValidationConstants.User.PasswordLowerCaseRegex)
                .WithMessage(ValidationMessages.PasswordLowerCase)
            .Matches(ValidationConstants.User.PasswordDigitRegex)
                .WithMessage(ValidationMessages.PasswordDigit);
    }
}
