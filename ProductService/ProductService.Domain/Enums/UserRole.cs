using System.Diagnostics.CodeAnalysis;

namespace UserService.Domain.Enums;

[Flags]
public enum UserRoles
{
    None = 0,
    Buyer = 1,
    Seller = 2,
    Admin = 4
}

[ExcludeFromCodeCoverage]
public static class UserRolePresets
{
    public const UserRoles BuyerOnly = UserRoles.Buyer;
    public const UserRoles SellerWithBuying = UserRoles.Buyer | UserRoles.Seller;
    public const UserRoles AdminAll = UserRoles.Buyer | UserRoles.Seller | UserRoles.Admin;
}
