using ProductService.BLL.Mappings;
using ProductService.BLL.Models;
using ProductService.BLL.Models.Category;
using ProductService.BLL.Models.Product;
using ProductService.BLL.Models.User;
using ProductService.DAL.Models;
using ProductService.Domain.Entities;
using ProductService.Domain.Enums;
using Shouldly;
using UserService.Domain.Entities;
using UserService.Domain.Enums;
using Xunit;

namespace ProductService.Tests.Mappings;

public class BllMappingProfileTests : MapperTestsBase<MappingProfile>
{
    [Fact]
    public void MapProductToProductModel_ShouldFlattenImages()
    {
        var sellerId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();

        var seller = new User
        {
            Id = sellerId,
            Username = "SellerBob",
            Email = "bob@test.com",
            PasswordHash = "hash",
            Profile = null!
        };
        seller.Profile = new UserProfile
        {
            UserId = sellerId,
            User = seller,

            Biography = "Bio",
            AvatarUrl = "http://avatar.com",
            RatingScore = 5.0
        };

        var entity = new Product
        {
            Id = Guid.NewGuid(),
            Title = "Test Product",
            Price = 100m,
            Description = "Desc",
            Priority = Priority.High,
            Status = ProductStatus.Available,
            CreationDate = DateTimeOffset.UtcNow,

            CategoryId = categoryId,
            SellerId = sellerId,

            Category = new Category { Id = categoryId, Name = "Tech" },
            Seller = seller,

            Images = new List<ProductImage>
            {
                new() { Url = "http://img1.com" },
                new() { Url = "http://img2.com" }
            }
        };

        var model = Mapper.Map<ProductModel>(entity);

        model.ShouldNotBeNull();
        model.ImageUrls.ShouldNotBeNull();
        model.ImageUrls.Count.ShouldBe(2);

        model.ImageUrls.ShouldContain("http://img1.com");
        model.ImageUrls.ShouldContain("http://img2.com");
    }

    [Fact]
    public void MapCreateProductModelWithImages_ShouldMapImagesAndSetDefaults()
    {
        var model = new CreateProductModel
        {
            Title = "New Product",
            Price = 100m,
            CategoryId = Guid.NewGuid(),
            SellerId = Guid.NewGuid(),
            Description = "Desc",

            ImageUrls = new List<string> { "http://img1.com", "http://img2.com" }
        };

        var entity = Mapper.Map<Product>(model);

        entity.Priority.ShouldBe(Priority.Low);
        entity.Status.ShouldBe(ProductStatus.Available);

        entity.Images.ShouldNotBeNull();
        entity.Images.Count().ShouldBe(2);

        var images = entity.Images.ToList();
        images[0].Url.ShouldBe("http://img1.com");
        images[1].Url.ShouldBe("http://img2.com");
    }

    [Fact]
    public void MapCreateProductModelWithNullImageUrls_ShouldReturnEmptyListNotThrow()
    {
        var model = new CreateProductModel
        {
            Title = "Null Images Product",
            Price = 100m,
            CategoryId = Guid.NewGuid(),
            SellerId = Guid.NewGuid(),

            ImageUrls = null
        };

        var entity = Mapper.Map<Product>(model);

        entity.Priority.ShouldBe(Priority.Low);
        entity.Status.ShouldBe(ProductStatus.Available);

        entity.Images.ShouldNotBeNull();
        entity.Images.ShouldBeEmpty();
    }

    [Fact]
    public void MapCreateProductModelWithEmptyImageUrls_ShouldReturnEmptyList()
    {
        var model = new CreateProductModel
        {
            Title = "Empty Images Product",
            Price = 100m,
            CategoryId = Guid.NewGuid(),
            SellerId = Guid.NewGuid(),

            ImageUrls = new List<string>()
        };

        var entity = Mapper.Map<Product>(model);

        entity.Images.ShouldNotBeNull();
        entity.Images.ShouldBeEmpty();
    }

    [Fact]
    public void MapUserToSellerModel_ShouldMap()
    {
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Username = "SellerOne",
            Email = "seller@test.com",
            PasswordHash = "hash",
            Profile = null!
        };
        user.Profile = new UserProfile
        {
            UserId = userId,
            User = user,

            Biography = "Bio",
            AvatarUrl = "http://avatar.com",
            RatingScore = 5.0
        };

        var model = Mapper.Map<SellerModel>(user);

        model.Id.ShouldBe(userId);
        model.Username.ShouldBe("SellerOne");
        model.Email.ShouldBe("seller@test.com");
    }

    [Fact]
    public void MapCreateCategoryModelToCategory_ShouldApplySentenceCase()
    {
        var model = new CreateCategoryModel
        {
            Name = "gaming LAPTOPS"
        };

        var entity = Mapper.Map<Category>(model);

        entity.Name.ShouldBe("Gaming laptops");
    }

    [Fact]
    public void MapUserWithProfileToUserModel_ShouldFlattenProfileData()
    {
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Username = "User",
            PasswordHash = "Password",
            Email = "user@test.com",
            Role = UserRoles.Buyer,
            RegistrationDate = DateTimeOffset.UtcNow,
            Profile = null!
        };

        user.Profile = new UserProfile
        {
            UserId = userId,
            User = user,
            Biography = "testing",
            AvatarUrl = "http://avatar.jpg",
            RatingScore = 4.5
        };

        var result = Mapper.Map<UserModel>(user);

        result.ShouldNotBeNull();
        result.Id.ShouldBe(userId);
        result.Username.ShouldBe("User");

        result.Biography.ShouldBe("testing");
        result.AvatarUrl.ShouldBe("http://avatar.jpg");
        result.RatingScore.ShouldBe(4.5);
    }

    [Fact]
    public void MapUserWithoutProfileToUserModel_ShouldSetDefaults()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = "NoProfileUser",
            PasswordHash = "Password",
            Email = "noprofile@test.com",
            Profile = null!
        };

        var result = Mapper.Map<UserModel>(user);

        result.ShouldNotBeNull();
        result.Username.ShouldBe("NoProfileUser");

        result.Biography.ShouldBeNull();
        result.AvatarUrl.ShouldBeNull();
        result.RatingScore.ShouldBe(0);
    }

    [Fact]
    public void MapCreateUserModelToUser_ShouldMapBasicFields()
    {
        var model = new CreateUserModel
        {
            Username = "NewUser",
            Email = "new@test.com",
            Password = "Password123!",
            Role = UserRoles.Seller
        };

        var result = Mapper.Map<User>(model);

        result.Username.ShouldBe("NewUser");
        result.Email.ShouldBe("new@test.com");
        result.Role.ShouldBe(UserRoles.Seller);
    }

    [Fact]
    public void MapPagedListToPagedResult_ShouldMapItemsDeeply()
    {
        var product = CreateValidProduct();
        product.Title = "Specific Title";
        product.Images = new List<ProductImage> { new() { Url = "http://img.com" } };

        var lastId = Guid.NewGuid();

        var pagedList = new PagedList<Product>
        {
            Items = new List<Product> { product },
            LastId = lastId
        };

        var result = Mapper.Map<PagedResult<ProductModel>>(pagedList);

        result.ContinuationToken.ShouldBe(lastId.ToString());

        result.Items.Count.ShouldBe(1);

        var item = result.Items[0];

        item.Id.ShouldBe(product.Id);
        item.Title.ShouldBe("Specific Title");
        item.ImageUrls.ShouldContain("http://img.com");
    }

    [Fact]
    public void MapPagedListWithNullLastId_ShouldMapContinuationTokenToNull()
    {
        var pagedList = new PagedList<Product>
        {
            Items = new List<Product> { CreateValidProduct() },
            LastId = null
        };

        var result = Mapper.Map<PagedResult<ProductModel>>(pagedList);

        result.ContinuationToken.ShouldBeNull();
        result.Items.Count.ShouldBe(1);
    }

    [Fact]
    public void MapEmptyPagedList_ShouldReturnEmptyResult()
    {
        var pagedList = new PagedList<Product>
        {
            Items = new List<Product>(),
            LastId = null
        };

        var result = Mapper.Map<PagedResult<ProductModel>>(pagedList);

        result.ShouldNotBeNull();
        result.Items.ShouldNotBeNull();
        result.Items.ShouldBeEmpty();
    }

    private static Product CreateValidProduct()
    {
        var sellerId = Guid.NewGuid();
        var seller = new User
        {
            Id = sellerId,
            Username = "U",
            Email = "E",
            PasswordHash = "P",
            Profile = null!
        };
        seller.Profile = new UserProfile { UserId = sellerId, User = seller };

        return new Product
        {
            Id = Guid.NewGuid(),
            Title = "P",
            Price = 10,
            Priority = Priority.Low,
            Status = ProductStatus.Available,
            CategoryId = Guid.NewGuid(),
            SellerId = sellerId,
            Category = new Category { Id = Guid.NewGuid(), Name = "C" },
            Seller = seller,
            Images = new List<ProductImage>()
        };
    }
}
