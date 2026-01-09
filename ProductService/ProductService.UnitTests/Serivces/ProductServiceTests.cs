using Moq;
using ProductService.BLL.Models;
using ProductService.BLL.Models.Product;
using ProductService.BLL.Services;
using ProductService.DAL.Models;
using ProductService.DAL.Repositories;
using ProductService.Domain.Enums;
using Shouldly;
using Xunit;

namespace ProductService.Tests.Services.Product;

public class ProductServiceTests : ServiceTestsBase
{
    private readonly Mock<IProductRepository> _repositoryMock;
    private readonly ProductsService _service;

    public ProductServiceTests()
    {
        _repositoryMock = new Mock<IProductRepository>();

        _service = new ProductsService(
            _repositoryMock.Object,
            MapperMock.Object
        );
    }

    [Fact]
    public async Task GetAll_ShouldReturnPagedResult()
    {
        int limit = 10;
        Guid? token = null;

        var entity = CreateProductEntity();
        entity.Title = "P1";

        var pagedList = new PagedList<Domain.Entities.Product>
        {
            Items = new List<Domain.Entities.Product> { entity },
            LastId = Guid.NewGuid()
        };

        var expectedModel = new PagedResult<ProductModel>
        {
            Items = new List<ProductModel> { new()
            {
                Title = "P1",
                Price = 10,
                Category = null!, Seller = null!
            }},
            ContinuationToken = pagedList.LastId.ToString()
        };

        _repositoryMock
            .Setup(r => r.GetPaged(limit, token, Ct))
            .ReturnsAsync(pagedList);

        MapperMock
            .Setup(m => m.Map<PagedResult<ProductModel>>(pagedList))
            .Returns(expectedModel);

        var result = await _service.GetAll(limit, token, Ct);

        result.ShouldBe(expectedModel);
        result.Items.Count.ShouldBe(1);
    }

    [Fact]
    public async Task Create_ShouldReturnCreatedModel()
    {
        var sellerId = Guid.NewGuid();
        var createModel = new CreateProductModel
        {
            Title = "New Product",
            Price = 100,
            CategoryId = Guid.NewGuid(),
            SellerId = sellerId
        };

        var entityToCreate = CreateProductEntity();
        entityToCreate.Title = "New Product";

        var createdEntity = CreateProductEntity();
        createdEntity.Id = Guid.NewGuid();
        createdEntity.Title = "New Product";
        createdEntity.SellerId = sellerId;

        var expectedModel = new ProductModel
        {
            Id = createdEntity.Id,
            Title = "New Product",
            Price = 100,
            Category = null!,
            Seller = null!
        };

        MapperMock
            .Setup(m => m.Map<Domain.Entities.Product>(createModel))
            .Returns(entityToCreate);

        _repositoryMock
            .Setup(r => r.Add(entityToCreate, Ct))
            .ReturnsAsync(createdEntity);

        MapperMock
            .Setup(m => m.Map<ProductModel>(createdEntity))
            .Returns(expectedModel);

        var result = await _service.Create(createModel, sellerId, Ct);

        result.ShouldBe(expectedModel);

        entityToCreate.SellerId.ShouldBe(sellerId);

        _repositoryMock.Verify(r => r.Add(entityToCreate, Ct), Times.Once);
    }

    [Fact]
    public async Task Create_WhenRepositoryFails_ThrowsInvalidOperationException()
    {
        var createModel = new CreateProductModel
        {
            Title = "Fail",
            Price = 10,
            CategoryId = Guid.NewGuid(),
            SellerId = Guid.NewGuid()
        };
        var entity = CreateProductEntity();

        MapperMock.Setup(m => m.Map<Domain.Entities.Product>(createModel)).Returns(entity);

        _repositoryMock
        .Setup(r => r.Add(entity, Ct))
        .ReturnsAsync((Domain.Entities.Product)null!);

        await Should.ThrowAsync<InvalidOperationException>(() =>
            _service.Create(createModel, Guid.NewGuid(), Ct));
    }

    [Fact]
    public async Task GetById_WhenExists_ReturnsModel()
    {
        var id = Guid.NewGuid();
        var entity = CreateProductEntity();
        entity.Id = id;

        var model = new ProductModel
        {
            Id = id,
            Title = "P",
            Price = 10,
            Category = null!,
            Seller = null!
        };

        _repositoryMock
            .Setup(r => r.GetById(id, true, Ct))
            .ReturnsAsync(entity);

        MapperMock.Setup(m => m.Map<ProductModel>(entity)).Returns(model);

        var result = await _service.GetById(id, Ct);

        result.ShouldBe(model);
    }

    [Fact]
    public async Task GetById_WhenNotExists_ThrowsKeyNotFoundException()
    {
        var id = Guid.NewGuid();
        _repositoryMock
            .Setup(r => r.GetById(id, true, Ct))
            .ReturnsAsync((Domain.Entities.Product?)null);

        await Should.ThrowAsync<KeyNotFoundException>(() => _service.GetById(id, Ct));
    }

    [Fact]
    public async Task Remove_WhenExists_CallsDelete()
    {
        var id = Guid.NewGuid();
        var entity = CreateProductEntity();
        entity.Id = id;

        _repositoryMock
            .Setup(r => r.GetById(id, false, Ct))
            .ReturnsAsync(entity);

        await _service.Remove(id, Ct);

        _repositoryMock.Verify(r => r.Delete(entity, Ct), Times.Once);
    }

    [Fact]
    public async Task Remove_WhenNotExists_ThrowsKeyNotFoundException()
    {
        var id = Guid.NewGuid();
        _repositoryMock
            .Setup(r => r.GetById(id, false, Ct))
            .ReturnsAsync((Domain.Entities.Product?)null);

        await Should.ThrowAsync<KeyNotFoundException>(() => _service.Remove(id, Ct));

        _repositoryMock.Verify(r => r.Delete(It.IsAny<ProductService.Domain.Entities.Product>(), Ct), Times.Never);
    }

    [Fact]
    public async Task Update_WhenExists_UpdatesAndReturnsModel()
    {
        var id = Guid.NewGuid();
        var updateModel = new UpdateProductModel
        {
            Id = id,
            Title = "Updated",
            Price = 999,
            Status = ProductStatus.Available,
            CategoryId = Guid.NewGuid(),
            ImageUrls = new List<string> { "url1" }
        };

        var existingEntity = CreateProductEntity();
        existingEntity.Id = id;
        existingEntity.Title = "Old";

        var expectedModel = new ProductModel
        {
            Id = id,
            Title = "Updated",
            Price = 999,
            Category = null!,
            Seller = null!
        };

        _repositoryMock
            .Setup(r => r.GetById(id, false, Ct))
            .ReturnsAsync(existingEntity);

        MapperMock
            .Setup(m => m.Map(updateModel, existingEntity))
            .Callback<UpdateProductModel, Domain.Entities.Product>((src, dest) =>
            {
                dest.Title = src.Title;
            });

        MapperMock
            .Setup(m => m.Map<ProductModel>(existingEntity))
            .Returns(expectedModel);

        var result = await _service.Update(updateModel, Ct);

        result.ShouldBe(expectedModel);

        _repositoryMock.Verify(r => r.Update(existingEntity, updateModel.ImageUrls, Ct), Times.Once);
    }

    [Fact]
    public async Task Update_WhenNotExists_ThrowsKeyNotFoundException()
    {
        var updateModel = new UpdateProductModel
        {
            Id = Guid.NewGuid(),
            Title = "U",
            Price = 1,
            Status = ProductStatus.Available,
            CategoryId = Guid.NewGuid()
        };

        _repositoryMock
            .Setup(r => r.GetById(updateModel.Id, false, Ct))
            .ReturnsAsync((Domain.Entities.Product?)null);

        await Should.ThrowAsync<KeyNotFoundException>(() => _service.Update(updateModel, Ct));

        _repositoryMock.Verify(r => r.Update(
            It.IsAny<Domain.Entities.Product>(),
            It.IsAny<ICollection<string>>(),
            Ct), Times.Never);
    }

    private static Domain.Entities.Product CreateProductEntity()
    {
        return new Domain.Entities.Product
        {
            Id = Guid.NewGuid(),
            Title = "Default",
            Price = 10,
            Priority = Priority.Medium,
            Status = ProductStatus.Available,
            CategoryId = Guid.NewGuid(),
            SellerId = Guid.NewGuid(),

            Category = null!,
            Seller = null!
        };
    }
}
