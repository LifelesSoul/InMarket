namespace ProductService.Domain.Constants;

public static class ValidationMessages
{
    public const string Required = "{PropertyName} is required.";
    public const string LengthRange = "{PropertyName} must be between {MinLength} and {MaxLength} characters.";
    public const string MaxLength = "{PropertyName} must not exceed {MaxLength} characters.";
    public const string MinLength = "{PropertyName} must be at least {MinLength} characters long.";
    public const string InvalidEmail = "Invalid email format.";

    // specific for User
    public const string UsernameRegex = "{PropertyName} can only contain letters, numbers, underscores, dots, and hyphens.";
    public const string PasswordUpperCase = "{PropertyName} must contain at least one uppercase letter.";
    public const string PasswordLowerCase = "{PropertyName} must contain at least one lowercase letter.";
    public const string PasswordDigit = "{PropertyName} must contain at least one number.";
}
