using ProductService.BLL.Extensions;
using Shouldly;
using UserService.Domain.Enums;
using Xunit;

namespace ProductService.Tests.Extensions;

public class UserRolesExtensionsTests
{
    private static readonly string[] BuyerOnly = ["Buyer"];
    private static readonly string[] BuyerAndSeller = ["Buyer", "Seller"];

    [Fact]
    public void ToMarketRoles_WhenBuyer_ReturnsBuyer()
    {
        UserRoles.Buyer.ToMarketRoles().ShouldBe(BuyerOnly);
    }

    [Fact]
    public void ToMarketRoles_WhenSellerOnly_AddsBuyerToo()
    {
        UserRoles.Seller.ToMarketRoles().ShouldBe(BuyerAndSeller);
    }

    [Fact]
    public void ToMarketRoles_WhenAdminInDatabase_DoesNotReturnAdmin()
    {
        UserRolePresets.AdminAll.ToMarketRoles().ShouldBe(BuyerAndSeller);
    }

    [Fact]
    public void ToMarketRoles_WhenNone_ReturnsEmpty()
    {
        UserRoles.None.ToMarketRoles().ShouldBeEmpty();
    }
}
