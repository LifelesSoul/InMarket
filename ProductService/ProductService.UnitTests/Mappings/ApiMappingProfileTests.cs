using ProductService.API.Models;
using ProductService.API.ViewModels.User;
using ProductService.BLL.Models;
using ProductService.BLL.Models.Category;
using ProductService.BLL.Models.Product;
using ProductService.Mappings;
using Shouldly;
using Xunit;

namespace ProductService.Tests.Mappings;

public class ApiMappingProfileTests : MapperTestsBase<MappingProfile>
{
    //Product
    [Fact]
    public void Map_ProductModel_To_ProductViewModel_ShouldMapCustomFields()
    {
        var model = new ProductModel
        {
            Id = Guid.NewGuid(),
            Title = "iPhone 15",
            Price = 1000m,
            ImageUrls = new List<string> { "http://main-image.com", "http://second.com" },

            Category = new ProductCategoryModel
            {
                Id = Guid.NewGuid(),
                Name = "Smartphones"
            },

            Seller = new SellerModel
            {
                Id = Guid.NewGuid(),
                Username = "AppleStore",
                Email = "store@apple.com"
            }
        };

        var viewModel = Mapper.Map<ProductViewModel>(model);

        viewModel.Id.ShouldBe(model.Id);
        viewModel.Title.ShouldBe("iPhone 15");
        viewModel.CategoryName.ShouldBe("Smartphones");
        viewModel.ImageUrl.ShouldBe("http://main-image.com");
    }

    [Fact]
    public void Map_ProductModel_WithMultipleImages_ShouldPickStrictlyTheFirstUrl()
    {
        var model = new ProductModel
        {
            Id = Guid.NewGuid(),
            Title = "Gallery Product",
            Price = 100,
            Category = new ProductCategoryModel { Id = Guid.NewGuid(), Name = "C" },
            Seller = new SellerModel { Id = Guid.NewGuid(), Username = "U", Email = "E" },

            ImageUrls = new List<string> { "http://FIRST.com", "http://SECOND.com", "http://THIRD.com" }
        };

        var viewModel = Mapper.Map<ProductViewModel>(model);

        viewModel.ImageUrl.ShouldBe("http://FIRST.com");
        viewModel.ImageUrl.ShouldNotBe("http://SECOND.com");

        model.ImageUrls.ShouldBe(["http://FIRST.com", "http://SECOND.com", "http://THIRD.com"]);
    }

    [Fact]
    public void Map_ProductModel_WithNoImages_To_ProductViewModel_ShouldHaveNullImage()
    {
        var model = new ProductModel
        {
            Id = Guid.NewGuid(),
            Title = "No Image Product",
            Price = 10m,
            ImageUrls = new List<string>(),

            Category = new ProductCategoryModel
            {
                Id = Guid.NewGuid(),
                Name = "Misc"
            },
            Seller = new SellerModel
            {
                Id = Guid.NewGuid(),
                Username = "SellerBob",
                Email = "bob@test.com"
            }
        };

        var viewModel = Mapper.Map<ProductViewModel>(model);

        viewModel.ImageUrl.ShouldBeNull();
    }

    [Fact]
    public void Map_ProductModel_WithNullCategory_ShouldMapCategoryNameToNull()
    {
        var model = new ProductModel
        {
            Id = Guid.NewGuid(),
            Title = "Orphan Product",
            Price = 100,
            ImageUrls = new List<string>(),
            Seller = new SellerModel { Id = Guid.NewGuid(), Username = "U", Email = "E" },

            Category = null!
        };

        var viewModel = Mapper.Map<ProductViewModel>(model);

        viewModel.CategoryName.ShouldBeNull();
    }

    [Fact]
    public void Map_ProductModel_WithNullImageUrlsList_ShouldMapImageUrlToNull()
    {
        var model = new ProductModel
        {
            Id = Guid.NewGuid(),
            Title = "Null Images Product",
            Price = 100,
            Category = new ProductCategoryModel { Id = Guid.NewGuid(), Name = "C" },
            Seller = new SellerModel { Id = Guid.NewGuid(), Username = "U", Email = "E" },

            ImageUrls = null!
        };

        var viewModel = Mapper.Map<ProductViewModel>(model);

        viewModel.ImageUrl.ShouldBeNull();
    }

    //Seller
    [Fact]
    public void Map_SellerModel_To_SellerViewModel_ShouldMap()
    {
        var model = new SellerModel
        {
            Id = Guid.NewGuid(),
            Username = "BestSeller",
            Email = "best@seller.com"
        };

        var viewModel = Mapper.Map<SellerViewModel>(model);

        viewModel.Id.ShouldBe(model.Id);

    }

    //Category
    [Fact]
    public void Map_CategoryModel_To_CategoryViewModel_ShouldMap()
    {
        var model = new CategoryModel
        {
            Id = Guid.NewGuid(),
            Name = "Laptops"
        };

        var viewModel = Mapper.Map<CategoryViewModel>(model);

        viewModel.Id.ShouldBe(model.Id);
        viewModel.Name.ShouldBe("Laptops");
    }

    //Paged
    [Fact]
    public void Map_PagedResult_ProductModel_To_PagedResult_ProductViewModel_ShouldMap()
    {
        var productModel = new ProductModel
        {
            Id = Guid.NewGuid(),
            Title = "P1",
            Price = 100,
            ImageUrls = new List<string> { "http://img.com" },

            Category = new ProductCategoryModel { Id = Guid.NewGuid(), Name = "C1" },
            Seller = new SellerModel { Id = Guid.NewGuid(), Username = "U1", Email = "E1" }
        };

        var sourcePagedResult = new PagedResult<ProductModel>
        {
            Items = new List<ProductModel> { productModel }
        };

        var result = Mapper.Map<PagedResult<ProductViewModel>>(sourcePagedResult);

        result.Items.Count.ShouldBe(1);
        result.Items[0].Title.ShouldBe("P1");
        result.Items[0].CategoryName.ShouldBe("C1");
    }

    [Fact]
    public void Map_EmptyPagedResult_ShouldReturnEmptyViewModelList_NotThrow()
    {
        var sourcePagedResult = new PagedResult<ProductModel>
        {
            Items = new List<ProductModel>(),
            ContinuationToken = null
        };

        var result = Mapper.Map<PagedResult<ProductViewModel>>(sourcePagedResult);

        result.ShouldNotBeNull();
        result.Items.ShouldNotBeNull();
        result.Items.Count.ShouldBe(0);
    }
}
