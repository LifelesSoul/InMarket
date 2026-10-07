using UserService.Domain.Enums;

namespace ProductService.BLL.Extensions;

public static class UserRolesExtensions
{
    public static IReadOnlyList<string> ToMarketRoles(this UserRoles roles)
    {
        var names = new List<string>();

        if (roles.HasFlag(UserRoles.Buyer) || roles.HasFlag(UserRoles.Seller))
        {
            names.Add(nameof(UserRoles.Buyer));
        }

        if (roles.HasFlag(UserRoles.Seller))
        {
            names.Add(nameof(UserRoles.Seller));
        }

        return names;
    }
}
