using AutoMapper;
using FluentValidation;
using Hangfire;
using Hangfire.Common;
using Hangfire.States;
using Microsoft.Extensions.Logging;
using Moq;
using ProductService.BLL.Constants;
using ProductService.BLL.Events;
using ProductService.BLL.Exceptions;
using ProductService.BLL.Models;
using ProductService.BLL.Models.Product;
using ProductService.BLL.Services;
using ProductService.BLL.Validators;
using ProductService.DAL.Models;
using ProductService.DAL.Repositories;
using ProductService.Domain.Enums;
using Shouldly;
using UserService.Domain.Entities;
using UserService.Domain.Enums;
using Xunit;

namespace ProductService.Tests.Services.Product;

public class ProductServiceTests : ServiceTestsBase
{
    private const string ExternalUserId = "auth0|123456";

    private readonly Mock<IProductRepository> _repositoryMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IBackgroundJobClient> _backgroundJobClientMock;
    private readonly Mock<ILogger<ProductsService>> _loggerMock;
    private readonly ProductsService _service;

    public ProductServiceTests()
    {
        _repositoryMock = new Mock<IProductRepository>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _backgroundJobClientMock = new Mock<IBackgroundJobClient>();
        _loggerMock = new Mock<ILogger<ProductsService>>();

        _service = new ProductsService(
            _repositoryMock.Object,
            _userRepositoryMock.Object,
            MapperMock.Object,
            _loggerMock.Object,
            _backgroundJobClientMock.Object,
            new CreateProductModelValidator(),
            new UpdateProductModelValidator()
        );
    }

    [Fact]
    public async Task GetAll_ShouldReturnPagedResult()
    {
        int limit = 10;
        Guid? token = null;

        var entity = CreateProductEntity();
        entity.Title = "Product 1";

        var pagedList = new PagedList<Domain.Entities.Product>
        {
            Items = new List<Domain.Entities.Product> { entity },
            LastId = Guid.NewGuid()
        };

        var expectedModel = new PagedResult<ProductModel>
        {
            Items = new List<ProductModel> { new()
            {
                Title = "Product 1",
                Price = 10,
                Category = null!,
                Seller = null!
            }},
            LastId = pagedList.LastId?.ToString()
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
    public async Task Create_ShouldReturnCreatedModel_AndEnqueueJob()
    {
        var sellerId = Guid.NewGuid();
        var externalUserId = "auth0|123456";
        SetupCaller(sellerId, UserRolePresets.SellerWithBuying);

        var createModel = new CreateProductModel
        {
            Title = "New Product",
            Price = 100,
            CategoryId = Guid.NewGuid()
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

        var notificationEvent = new CreateNotificationEvent
        {
            Title = NotificationMessages.ProductCreatedTitle,
            Message = NotificationMessages.GetProductCreatedMessage(createdEntity.Title),
            UserId = sellerId,
            ExternalId = externalUserId
        };

        MapperMock.Setup(m => m.Map<Domain.Entities.Product>(createModel)).Returns(entityToCreate);

        _repositoryMock.Setup(r => r.Add(entityToCreate, Ct)).ReturnsAsync(createdEntity);

        MapperMock
            .Setup(m => m.Map<CreateNotificationEvent>(
                createdEntity,
                It.IsAny<Action<IMappingOperationOptions<object, CreateNotificationEvent>>>()))
            .Callback<object, Action<IMappingOperationOptions<object, CreateNotificationEvent>>>((src, optionsAction) =>
            {
                var optionsMock = new Mock<IMappingOperationOptions<object, CreateNotificationEvent>>();
                var itemsDictionary = new Dictionary<string, object>();
                optionsMock.SetupGet(x => x.Items).Returns(itemsDictionary);
                optionsAction(optionsMock.Object);
            })
            .Returns(notificationEvent);

        MapperMock.Setup(m => m.Map<ProductModel>(createdEntity)).Returns(expectedModel);

        var result = await _service.Create(createModel, Owner, Ct);

        result.ShouldBe(expectedModel);
        entityToCreate.SellerId.ShouldBe(sellerId);
        _repositoryMock.Verify(r => r.Add(entityToCreate, Ct), Times.Once);

        _backgroundJobClientMock.Verify(x => x.Create(
            It.Is<Job>(job =>
                job.Type == typeof(IEventPublisher) &&
                job.Method.Name == nameof(IEventPublisher.PublishNotification) &&
                (CreateNotificationEvent)job.Args[0] == notificationEvent
            ),
            It.IsAny<EnqueuedState>()
        ), Times.Once);
    }

    [Fact]
    public async Task Create_WhenRepositoryFails_ThrowsInvalidOperationException()
    {
        var createModel = new CreateProductModel
        {
            Title = "Fail Product",
            Price = 10,
            CategoryId = Guid.NewGuid()
        };
        var entity = CreateProductEntity();
        SetupCaller(Guid.NewGuid(), UserRolePresets.SellerWithBuying);

        MapperMock.Setup(m => m.Map<Domain.Entities.Product>(createModel)).Returns(entity);
        _repositoryMock.Setup(r => r.Add(entity, Ct)).ReturnsAsync((Domain.Entities.Product)null!);

        await Should.ThrowAsync<InvalidOperationException>(() => _service.Create(createModel, Owner, Ct));

        _backgroundJobClientMock.Verify(x => x.Create(It.IsAny<Job>(), It.IsAny<IState>()), Times.Never);
    }

    [Fact]
    public async Task Create_WhenHangfireFails_ShouldLogAndReturnModel()
    {
        var sellerId = Guid.NewGuid();
        SetupCaller(sellerId, UserRolePresets.SellerWithBuying);
        var createModel = new CreateProductModel
        {
            Title = "New Product",
            Price = 100,
            CategoryId = Guid.NewGuid()
        };

        var entityToCreate = CreateProductEntity();
        var createdEntity = CreateProductEntity();
        createdEntity.Id = Guid.NewGuid();

        var expectedModel = new ProductModel
        {
            Id = createdEntity.Id,
            Title = "New Product",
            Price = 100,
            Category = null!,
            Seller = null!
        };

        MapperMock.Setup(m => m.Map<Domain.Entities.Product>(createModel)).Returns(entityToCreate);
        _repositoryMock.Setup(r => r.Add(entityToCreate, Ct)).ReturnsAsync(createdEntity);

        MapperMock.Setup(m => m.Map<CreateNotificationEvent>(It.IsAny<object>(), It.IsAny<Action<IMappingOperationOptions<object, CreateNotificationEvent>>>()))
            .Returns(new CreateNotificationEvent
            {
                Title = "Dummy",
                Message = "Dummy",
                UserId = Guid.NewGuid(),
                ExternalId = "Dummy"
            });

        MapperMock.Setup(m => m.Map<ProductModel>(createdEntity)).Returns(expectedModel);

        _backgroundJobClientMock.Setup(x => x.Create(It.IsAny<Job>(), It.IsAny<IState>()))
            .Throws(new Exception("Hangfire error"));

        var result = await _service.Create(createModel, Owner, Ct);

        result.ShouldBe(expectedModel);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("failed to enqueue notification event")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
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
            Title = "Product Title",
            Price = 10,
            Category = null!,
            Seller = null!
        };

        _repositoryMock
            .Setup(r => r.GetById(id, Ct, true))
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
            .Setup(r => r.GetById(id, Ct, true))
            .ReturnsAsync((Domain.Entities.Product?)null);

        var exception = await Should.ThrowAsync<KeyNotFoundException>(() =>
            _service.GetById(id, Ct));

        exception.Message.ShouldBe($"Product {id} not found");
    }

    [Fact]
    public async Task Remove_WhenExists_EnqueuesJob_AndSetsContextItems()
    {
        var id = Guid.NewGuid();
        var entity = CreateProductEntity();
        entity.Id = id;
        entity.Title = "Test Product";
        var externalUserId = "auth0|123456";
        SetupCaller(entity.SellerId, UserRolePresets.SellerWithBuying);

        _repositoryMock
            .Setup(r => r.GetById(id, Ct, false))
            .ReturnsAsync(entity);

        var notificationEvent = new CreateNotificationEvent
        {
            Title = NotificationMessages.ProductDeletedTitle,
            Message = NotificationMessages.GetProductDeletedMessage(entity.Title),
            UserId = id,
            ExternalId = externalUserId
        };

        MapperMock
            .Setup(m => m.Map<CreateNotificationEvent>(
                entity,
                It.IsAny<Action<IMappingOperationOptions<object, CreateNotificationEvent>>>()))
            .Callback<object, Action<IMappingOperationOptions<object, CreateNotificationEvent>>>((src, optionsAction) =>
            {
                var optionsMock = new Mock<IMappingOperationOptions<object, CreateNotificationEvent>>();
                var itemsDictionary = new Dictionary<string, object>();
                optionsMock.SetupGet(x => x.Items).Returns(itemsDictionary);

                optionsAction(optionsMock.Object);

                itemsDictionary[nameof(CreateNotificationEvent.Title)].ShouldBe(NotificationMessages.ProductDeletedTitle);
                itemsDictionary[nameof(CreateNotificationEvent.Message)].ShouldBe(NotificationMessages.GetProductDeletedMessage(entity.Title));
                itemsDictionary[nameof(CreateNotificationEvent.ExternalId)].ShouldBe(externalUserId);
            })
            .Returns(notificationEvent);

        await _service.Remove(id, Owner, Ct);

        _repositoryMock.Verify(r => r.Delete(entity, Ct), Times.Once);

        _backgroundJobClientMock.Verify(x => x.Create(
            It.Is<Job>(job =>
                job.Method.Name == nameof(IEventPublisher.PublishNotification) &&
                (CreateNotificationEvent)job.Args[0] == notificationEvent
            ),
            It.IsAny<EnqueuedState>()
        ), Times.Once);
    }

    [Fact]
    public async Task Remove_WhenNotExists_ThrowsKeyNotFoundException()
    {
        var id = Guid.NewGuid();

        _repositoryMock
            .Setup(r => r.GetById(id, Ct, false))
            .ReturnsAsync((Domain.Entities.Product?)null);

        var exception = await Should.ThrowAsync<KeyNotFoundException>(() =>
            _service.Remove(id, Owner, Ct));

        exception.Message.ShouldBe($"Product {id} not found");

        _repositoryMock.Verify(r => r.Delete(It.IsAny<Domain.Entities.Product>(), Ct), Times.Never);
        _backgroundJobClientMock.Verify(x => x.Create(It.IsAny<Job>(), It.IsAny<IState>()), Times.Never);
    }

    [Fact]
    public async Task Update_WhenExists_UpdatesAndEnqueuesJob_AndSetsContextItems()
    {
        var id = Guid.NewGuid();
        var externalUserId = "auth0|123456";

        var updateModel = new UpdateProductModel
        {
            Id = id,
            Title = "Updated Product Title",
            Price = 999,
            Status = ProductStatus.Available,
            CategoryId = Guid.NewGuid(),
            ImageUrls = new List<string> { "url1" }
        };

        var existingEntity = CreateProductEntity();
        existingEntity.Id = id;
        existingEntity.Title = "Old Product Title";
        SetupCaller(existingEntity.SellerId, UserRolePresets.SellerWithBuying);

        var expectedModel = new ProductModel
        {
            Id = id,
            Title = "Updated Product Title",
            Price = 999,
            Category = null!,
            Seller = null!
        };

        var notificationEvent = new CreateNotificationEvent
        {
            Title = NotificationMessages.ProductUpdatedTitle,
            Message = NotificationMessages.GetProductUpdatedMessage(updateModel.Title),
            UserId = id,
            ExternalId = externalUserId
        };

        _repositoryMock.Setup(r => r.GetById(id, Ct, false)).ReturnsAsync(existingEntity);

        MapperMock
            .Setup(m => m.Map(updateModel, existingEntity))
            .Callback<UpdateProductModel, Domain.Entities.Product>((src, dest) =>
            {
                dest.Title = src.Title;
            });

        MapperMock
            .Setup(m => m.Map<CreateNotificationEvent>(
                existingEntity,
                It.IsAny<Action<IMappingOperationOptions<object, CreateNotificationEvent>>>()))
            .Callback<object, Action<IMappingOperationOptions<object, CreateNotificationEvent>>>((src, optionsAction) =>
            {
                var optionsMock = new Mock<IMappingOperationOptions<object, CreateNotificationEvent>>();
                var itemsDictionary = new Dictionary<string, object>();
                optionsMock.SetupGet(x => x.Items).Returns(itemsDictionary);

                optionsAction(optionsMock.Object);

                itemsDictionary[nameof(CreateNotificationEvent.Title)].ShouldBe(NotificationMessages.ProductUpdatedTitle);
                itemsDictionary[nameof(CreateNotificationEvent.Message)].ShouldBe(NotificationMessages.GetProductUpdatedMessage(existingEntity.Title));
                itemsDictionary[nameof(CreateNotificationEvent.ExternalId)].ShouldBe(externalUserId);
            })
            .Returns(notificationEvent);

        MapperMock.Setup(m => m.Map<ProductModel>(existingEntity)).Returns(expectedModel);

        await _service.Update(updateModel, Owner, Ct);

        _repositoryMock.Verify(r => r.Update(existingEntity, updateModel.ImageUrls, Ct), Times.Once);

        _backgroundJobClientMock.Verify(x => x.Create(
            It.Is<Job>(job =>
                job.Method.Name == nameof(IEventPublisher.PublishNotification) &&
                (CreateNotificationEvent)job.Args[0] == notificationEvent
            ),
            It.IsAny<EnqueuedState>()
        ), Times.Once);
    }

    [Fact]
    public async Task Update_WhenNotExists_ThrowsKeyNotFoundException()
    {
        var updateModel = new UpdateProductModel
        {
            Id = Guid.NewGuid(),
            Title = "Update Test",
            Price = 1,
            Status = ProductStatus.Available,
            CategoryId = Guid.NewGuid()
        };

        _repositoryMock.Setup(r => r.GetById(updateModel.Id, Ct, false)).ReturnsAsync((Domain.Entities.Product?)null);

        await Should.ThrowAsync<KeyNotFoundException>(() => _service.Update(updateModel, Owner, Ct));
    }

    [Fact]
    public async Task Create_WhenCallerIsNotSeller_ThrowsForbiddenException()
    {
        SetupCaller(Guid.NewGuid(), UserRoles.Buyer);

        var createModel = new CreateProductModel
        {
            Title = "New Product",
            Price = 100,
            CategoryId = Guid.NewGuid()
        };

        await Should.ThrowAsync<ForbiddenException>(() => _service.Create(createModel, Owner, Ct));

        _repositoryMock.Verify(r => r.Add(It.IsAny<Domain.Entities.Product>(), Ct), Times.Never);
    }

    [Fact]
    public async Task Create_WhenCallerHasNoUser_ThrowsForbiddenException()
    {
        _userRepositoryMock
            .Setup(r => r.GetByExternalId(ExternalUserId, Ct))
            .ReturnsAsync((User?)null);

        var createModel = new CreateProductModel
        {
            Title = "New Product",
            Price = 100,
            CategoryId = Guid.NewGuid()
        };

        await Should.ThrowAsync<ForbiddenException>(() => _service.Create(createModel, Owner, Ct));
    }

    [Fact]
    public async Task Remove_WhenCallerIsNotOwner_ThrowsForbiddenException()
    {
        var entity = CreateProductEntity();
        _repositoryMock.Setup(r => r.GetById(entity.Id, Ct, false)).ReturnsAsync(entity);
        SetupCaller(Guid.NewGuid(), UserRolePresets.SellerWithBuying);

        await Should.ThrowAsync<ForbiddenException>(() => _service.Remove(entity.Id, Owner, Ct));

        _repositoryMock.Verify(r => r.Delete(It.IsAny<Domain.Entities.Product>(), Ct), Times.Never);
    }

    [Fact]
    public async Task Remove_WhenCallerIsAdmin_SkipsOwnerCheck()
    {
        var entity = CreateProductEntity();
        _repositoryMock.Setup(r => r.GetById(entity.Id, Ct, false)).ReturnsAsync(entity);

        MapperMock
            .Setup(m => m.Map<CreateNotificationEvent>(
                entity,
                It.IsAny<Action<IMappingOperationOptions<object, CreateNotificationEvent>>>()))
            .Returns(new CreateNotificationEvent
            {
                Title = NotificationMessages.ProductDeletedTitle,
                Message = NotificationMessages.GetProductDeletedMessage(entity.Title),
                UserId = entity.SellerId,
                ExternalId = "auth0|admin"
            });

        await _service.Remove(entity.Id, new Caller("auth0|admin", IsAdmin: true), Ct);

        _repositoryMock.Verify(r => r.Delete(entity, Ct), Times.Once);
        _userRepositoryMock.Verify(r => r.GetByExternalId(It.IsAny<string>(), Ct), Times.Never);
    }

    [Fact]
    public async Task Update_WhenCallerIsNotOwner_ThrowsForbiddenException()
    {
        var entity = CreateProductEntity();
        _repositoryMock.Setup(r => r.GetById(entity.Id, Ct, false)).ReturnsAsync(entity);
        SetupCaller(Guid.NewGuid(), UserRolePresets.SellerWithBuying);

        var updateModel = new UpdateProductModel
        {
            Id = entity.Id,
            Title = "Hacked",
            Price = 1,
            Status = ProductStatus.Available,
            CategoryId = entity.CategoryId
        };

        await Should.ThrowAsync<ForbiddenException>(() => _service.Update(updateModel, Owner, Ct));

        entity.Title.ShouldBe("Default Product");
        _repositoryMock.Verify(r => r.Update(It.IsAny<Domain.Entities.Product>(), It.IsAny<IEnumerable<string>>(), Ct), Times.Never);
    }

    [Fact]
    public async Task Create_WhenModelIsInvalid_ThrowsValidationException()
    {
        SetupCaller(Guid.NewGuid(), UserRolePresets.SellerWithBuying);

        var createModel = new CreateProductModel
        {
            Title = "",
            Price = 0,
            CategoryId = Guid.Empty
        };

        await Should.ThrowAsync<ValidationException>(() => _service.Create(createModel, Owner, Ct));

        _repositoryMock.Verify(r => r.Add(It.IsAny<Domain.Entities.Product>(), Ct), Times.Never);
    }

    [Fact]
    public async Task Create_WhenImageUrlIsNotHttp_ThrowsValidationException()
    {
        SetupCaller(Guid.NewGuid(), UserRolePresets.SellerWithBuying);

        var createModel = new CreateProductModel
        {
            Title = "New Product",
            Price = 100,
            CategoryId = Guid.NewGuid(),
            ImageUrls = ["https://images.example.com/1.png", "ftp://images.example.com/2.png"]
        };

        await Should.ThrowAsync<ValidationException>(() => _service.Create(createModel, Owner, Ct));

        _repositoryMock.Verify(r => r.Add(It.IsAny<Domain.Entities.Product>(), Ct), Times.Never);
    }

    [Fact]
    public async Task Update_WhenModelIsInvalid_ThrowsValidationException()
    {
        var entity = CreateProductEntity();
        _repositoryMock.Setup(r => r.GetById(entity.Id, Ct, false)).ReturnsAsync(entity);
        SetupCaller(entity.SellerId, UserRolePresets.SellerWithBuying);

        var updateModel = new UpdateProductModel
        {
            Id = entity.Id,
            Title = "Updated",
            Price = 10.999m,
            Status = (ProductStatus)42,
            CategoryId = entity.CategoryId
        };

        await Should.ThrowAsync<ValidationException>(() => _service.Update(updateModel, Owner, Ct));

        entity.Title.ShouldBe("Default Product");
        _repositoryMock.Verify(r => r.Update(It.IsAny<Domain.Entities.Product>(), It.IsAny<IEnumerable<string>>(), Ct), Times.Never);
    }

    private static Caller Owner => new(ExternalUserId, IsAdmin: false);

    private User SetupCaller(Guid id, UserRoles role)
    {
        var user = new User
        {
            Id = id,
            ExternalId = ExternalUserId,
            Username = "seller",
            Email = "seller@test.com",
            Role = role,
            Profile = null!
        };

        _userRepositoryMock
            .Setup(r => r.GetByExternalId(ExternalUserId, Ct))
            .ReturnsAsync(user);

        return user;
    }

    private static Domain.Entities.Product CreateProductEntity()
    {
        return new Domain.Entities.Product
        {
            Id = Guid.NewGuid(),
            Title = "Default Product",
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
