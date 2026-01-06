using FluentValidation;
using FluentValidation.Results;
using Moq;
using ProductService.BLL.Models.Category;
using ProductService.BLL.Services;
using ProductService.DAL.Repositories;
using Shouldly;
using Xunit;

namespace ProductService.Tests.Services.Category;

public class CategoryServiceTests : ServiceTestsBase
{
    private readonly Mock<ICategoryRepository> _repositoryMock;
    private readonly Mock<IValidator<CreateCategoryModel>> _createValidatorMock;
    private readonly Mock<IValidator<CategoryModel>> _updateValidatorMock;
    private readonly CategoryService _service;

    public CategoryServiceTests()
    {
        _repositoryMock = new Mock<ICategoryRepository>();
        _createValidatorMock = new Mock<IValidator<CreateCategoryModel>>();
        _updateValidatorMock = new Mock<IValidator<CategoryModel>>();

        _service = new CategoryService(
            _repositoryMock.Object,
            MapperMock.Object,
            _createValidatorMock.Object,
            _updateValidatorMock.Object
        );
    }

    [Fact]
    public async Task GetAll_ShouldReturnMappedModels()
    {
        var entities = new List<Domain.Entities.Category>
        {
            new() { Id = Guid.NewGuid(), Name = "C1" },
            new() { Id = Guid.NewGuid(), Name = "C2" }
        };

        var expectedModels = new List<CategoryModel>
        {
            new() { Id = entities[0].Id, Name = "C1" },
            new() { Id = entities[1].Id, Name = "C2" }
        };

        _repositoryMock
            .Setup(r => r.GetAll(Ct))
            .ReturnsAsync(entities);

        MapperMock
            .Setup(m => m.Map<List<CategoryModel>>(entities))
            .Returns(expectedModels);

        var result = await _service.GetAll(Ct);

        result.ShouldBe(expectedModels);
        result.Count.ShouldBe(2);
    }

    [Fact]
    public async Task GetById_WhenExists_ReturnsModel()
    {
        var id = Guid.NewGuid();
        var entity = new Domain.Entities.Category { Id = id, Name = "Tech" };
        var expectedModel = new CategoryModel { Id = id, Name = "Tech" };

        _repositoryMock
            .Setup(r => r.GetById(id, true, Ct))
            .ReturnsAsync(entity);

        MapperMock
            .Setup(m => m.Map<CategoryModel>(entity))
            .Returns(expectedModel);

        var result = await _service.GetById(id, Ct);

        result.ShouldBe(expectedModel);
    }

    [Fact]
    public async Task GetById_WhenNotExists_ThrowsKeyNotFoundException()
    {
        var id = Guid.NewGuid();

        _repositoryMock
            .Setup(r => r.GetById(id, true, Ct))
            .ReturnsAsync((Domain.Entities.Category?)null);

        await Should.ThrowAsync<KeyNotFoundException>(() => _service.GetById(id, Ct));
    }

    [Fact]
    public async Task Create_WhenValidationSucceeds_ReturnsCreatedModel()
    {
        var createModel = new CreateCategoryModel { Name = "New" };
        var entityToCreate = new Domain.Entities.Category { Name = "New" };
        var createdEntity = new Domain.Entities.Category { Id = Guid.NewGuid(), Name = "New" };
        var expectedModel = new CategoryModel { Id = createdEntity.Id, Name = "New" };

        _createValidatorMock
            .Setup(v => v.ValidateAsync(createModel, Ct))
            .ReturnsAsync(new ValidationResult());

        MapperMock
            .Setup(m => m.Map<Domain.Entities.Category>(createModel))
            .Returns(entityToCreate);

        _repositoryMock
            .Setup(r => r.Add(entityToCreate, Ct))
            .ReturnsAsync(createdEntity);

        MapperMock
            .Setup(m => m.Map<CategoryModel>(createdEntity))
            .Returns(expectedModel);

        var result = await _service.Create(createModel, Ct);

        result.ShouldBe(expectedModel);
        _repositoryMock.Verify(r => r.Add(entityToCreate, Ct), Times.Once);
    }

    [Fact]
    public async Task Create_WhenValidationFails_ThrowsValidationException()
    {
        var createModel = new CreateCategoryModel { Name = "" };
        var failedResult = new ValidationResult(new[] { new ValidationFailure("Name", "Required") });

        _createValidatorMock
            .Setup(v => v.ValidateAsync(createModel, Ct))
            .ReturnsAsync(failedResult);

        await Should.ThrowAsync<ValidationException>(() => _service.Create(createModel, Ct));

        _repositoryMock.Verify(r => r.Add(It.IsAny<Domain.Entities.Category>(), Ct), Times.Never);
    }

    [Fact]
    public async Task Create_WhenRepositoryReturnsNull_ThrowsInvalidOperationException()
    {
        var createModel = new CreateCategoryModel { Name = "Valid" };
        var entity = new Domain.Entities.Category { Name = "Valid" };

        _createValidatorMock
            .Setup(v => v.ValidateAsync(createModel, Ct))
            .ReturnsAsync(new ValidationResult());

        MapperMock.Setup(m => m.Map<Domain.Entities.Category>(createModel)).Returns(entity);

        _repositoryMock
            .Setup(r => r.Add(entity, Ct))
            .ReturnsAsync((Domain.Entities.Category)null!);

        await Should.ThrowAsync<InvalidOperationException>(() => _service.Create(createModel, Ct));
    }

    [Fact]
    public async Task Update_WhenEntityExistsAndValid_UpdatesAndReturnsModel()
    {
        var id = Guid.NewGuid();
        var updateModel = new CategoryModel { Id = id, Name = "Updated" };
        var existingEntity = new Domain.Entities.Category { Id = id, Name = "Old" };

        _repositoryMock
            .Setup(r => r.GetById(id, false, Ct))
            .ReturnsAsync(existingEntity);

        _updateValidatorMock
            .Setup(v => v.ValidateAsync(updateModel, Ct))
            .ReturnsAsync(new ValidationResult());

        MapperMock
            .Setup(m => m.Map(updateModel, existingEntity))
            .Callback<CategoryModel, Domain.Entities.Category>((src, dest) =>
            {
                dest.Name = src.Name;
            });

        var expectedModel = new CategoryModel { Id = id, Name = "Updated" };
        MapperMock
            .Setup(m => m.Map<CategoryModel>(existingEntity))
            .Returns(expectedModel);

        var result = await _service.Update(updateModel, Ct);

        result.ShouldBe(expectedModel);
        _repositoryMock.Verify(r => r.Update(existingEntity, Ct), Times.Once);
    }

    [Fact]
    public async Task Update_WhenEntityNotExists_ThrowsKeyNotFoundException()
    {
        var updateModel = new CategoryModel { Id = Guid.NewGuid(), Name = "U" };

        _repositoryMock
            .Setup(r => r.GetById(updateModel.Id, false, Ct))
            .ReturnsAsync((Domain.Entities.Category?)null);

        await Should.ThrowAsync<KeyNotFoundException>(() => _service.Update(updateModel, Ct));

        _updateValidatorMock.Verify(v => v.ValidateAsync(It.IsAny<CategoryModel>(), Ct), Times.Never);
    }

    [Fact]
    public async Task Update_WhenValidationFails_ThrowsValidationException()
    {
        var id = Guid.NewGuid();
        var updateModel = new CategoryModel { Id = id, Name = "A" };
        var existingEntity = new Domain.Entities.Category { Id = id, Name = "Old" };

        _repositoryMock
            .Setup(r => r.GetById(id, false, Ct))
            .ReturnsAsync(existingEntity);

        var validationResultWithErrors = new ValidationResult(new[] { new ValidationFailure("Name", "Name is too short") });

        _updateValidatorMock
            .Setup(v => v.ValidateAsync(updateModel, Ct))
            .ReturnsAsync(new ValidationResult());

        await Should.ThrowAsync<ValidationException>(() => _service.Update(updateModel, Ct));

        _repositoryMock.Verify(r => r.Update(It.IsAny<Domain.Entities.Category>(), Ct), Times.Never);
    }

    [Fact]
    public async Task Remove_WhenExists_CallsDelete()
    {
        var id = Guid.NewGuid();
        var entity = new Domain.Entities.Category { Id = id, Name = "To Delete" };

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
            .ReturnsAsync((Domain.Entities.Category?)null);

        await Should.ThrowAsync<KeyNotFoundException>(() => _service.Remove(id, Ct));

        _repositoryMock.Verify(r => r.Delete(It.IsAny<Domain.Entities.Category>(), Ct), Times.Never);
    }
}
