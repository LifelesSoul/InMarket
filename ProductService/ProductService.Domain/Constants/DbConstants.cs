namespace ProductService.Domain.Constants;

public static class DbConstants
{
    public const string MoneyType = "decimal(18,2)";

    public const string StatusColumnType = "nvarchar(50)";

    public const string RatingScoreType = "float";

    public const int UserNameTextLength = 100;
    public const int NameTextLength = 100;
    public const int EmailTextLength = 250;
    public const int AvatarURLLength = 500;
    public const int BiographyTextLength = 1000;
    public const int TitleTextLength = 250;
}
