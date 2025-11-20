namespace UserService.Domain.Enums;

[Flags]
public enum UserRole
{
    Buyer = 1,
    Seller = 2,
    Admin = 4
}

public static class UserRoles
{
    public const UserRole BuyerOnly = UserRole.Buyer;
    public const UserRole SellerWithBuying = UserRole.Buyer | UserRole.Seller;
    public const UserRole AdminAll = UserRole.Buyer | UserRole.Seller | UserRole.Admin;
}
