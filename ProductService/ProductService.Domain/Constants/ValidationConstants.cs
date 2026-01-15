using System.Diagnostics.CodeAnalysis;

namespace ProductService.Domain.Constants;

[ExcludeFromCodeCoverage]
[SuppressMessage("Security", "S2068:Hard-coded credentials", Justification = "These are validation messages, not secrets.")]
public static class ValidationConstants
{
    public static class Category
    {
        public const int NameMinLength = 2;
        public const int NameMaxLength = 50;
    }

    public static class User
    {
        public const int UsernameMinLength = 3;
        public const int UsernameMaxLength = 50;
        public const string UsernameRegex = "^[a-zA-Z0-9_.-]+$";

        public const int EmailMaxLength = 100;

        public const int PasswordMinLength = 6;
        public const int PasswordMaxLength = 50;
        public const string PasswordUpperCaseRegex = "[A-Z]";
        public const string PasswordLowerCaseRegex = "[a-z]";
        public const string PasswordDigitRegex = "[0-9]";
    }
}
