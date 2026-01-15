using FluentValidation;
using FluentValidation.Results;
using Moq;
using ProductService.BLL.Models.User;
using ProductService.BLL.Services;
using ProductService.DAL.Repositories;
using ProductService.Domain.Constants;
using Shouldly;
using UserService.Domain.Entities;
using UserService.Domain.Enums;
using Xunit;

namespace ProductService.Tests.Services;

public class UserServiceTests : ServiceTestsBase
{
    private readonly Mock<IUserRepository> _repositoryMock;
    private readonly Mock<IValidator<CreateUserModel>> _createValidatorMock;
    private readonly Mock<IValidator<UpdateUserModel>> _updateValidatorMock;
    private readonly UsersService _service;

    public UserServiceTests()
    {
        _repositoryMock = new Mock<IUserRepository>();
        _createValidatorMock = new Mock<IValidator<CreateUserModel>>();
        _updateValidatorMock = new Mock<IValidator<UpdateUserModel>>();

        _service = new UsersService(
            _repositoryMock.Object,
            MapperMock.Object,
            _createValidatorMock.Object,
            _updateValidatorMock.Object
        );
    }

    [Fact]
    public async Task GetAll_ShouldReturnMappedUsers_WhenCalled()
    {
        var user = CreateUser();

        var users = new List<User> { user };

        var userModels = new List<UserModel>
        {
            new()
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role
            }
        };

        _repositoryMock
            .Setup(r => r.GetPaged(1, 10, Ct, false))
            .ReturnsAsync(users);

        MapperMock
            .Setup(m => m.Map<IList<UserModel>>(users))
            .Returns(userModels);

        var result = await _service.GetAll(1, 10, Ct);

        result.ShouldBe(userModels);
        result.Count.ShouldBe(1);
    }

    [Fact]
    public async Task GetAll_ShouldUseDefaultPagination_WhenInputsAreInvalid()
    {
        _repositoryMock
            .Setup(r => r.GetPaged(PaginationConstants.DefaultPageNumber, PaginationConstants.DefaultPageSize, Ct, false))
            .ReturnsAsync(new List<User>());

        MapperMock
            .Setup(m => m.Map<IList<UserModel>>(It.IsAny<IList<User>>()))
            .Returns(new List<UserModel>());

        await _service.GetAll(-1, 0, Ct);

        _repositoryMock.Verify(r => r.GetPaged(PaginationConstants.DefaultPageNumber, PaginationConstants.DefaultPageSize, Ct, false), Times.Once);
    }

    [Fact]
    public async Task GetById_ShouldReturnUser_WhenUserExists()
    {
        var id = Guid.NewGuid();
        var user = CreateUser(id: id);

        user.Profile = new UserProfile
        {
            UserId = user.Id,
            User = user,
            Biography = "Manual Bio",
            AvatarUrl = "http://manual.com/img.jpg",
            RatingScore = 5
        };

        var userModel = new UserModel
        {
            Id = id,
            Username = "TestUser",
            Email = "test@test.com",
            Role = UserRoles.Seller,

            RegistrationDate = DateTimeOffset.UtcNow,

            AvatarUrl = "http://test.com/avatar.jpg",
            Biography = "Test Bio",
            RatingScore = 5.0
        };

        _repositoryMock
            .Setup(r => r.GetById(id, Ct, true))
            .ReturnsAsync(user);

        MapperMock.Setup(m => m.Map<UserModel>(user)).Returns(userModel);

        var result = await _service.GetById(id, Ct);

        result.ShouldNotBeNull();
        result.Id.ShouldBe(id);
    }

    [Fact]
    public async Task GetById_ShouldReturnNull_WhenUserDoesNotExist()
    {
        var id = Guid.NewGuid();
        _repositoryMock
            .Setup(r => r.GetById(id, Ct, true))
            .ReturnsAsync((User?)null);

        var result = await _service.GetById(id, Ct);

        result.ShouldBeNull();
    }

    [Fact]
    public async Task Create_ShouldCreateUser_WhenModelIsValid()
    {
        var model = new CreateUserModel
        {
            Email = "test@test.com",
            Password = "Password123",
            Username = "TestUser",
            Role = UserRoles.Seller
        };

        var userEntity = CreateUser(email: model.Email, role: model.Role);

        var expectedResult = new UserModel
        {
            Id = userEntity.Id,
            Username = "TestUser",
            Email = "test@test.com",
            Role = UserRoles.Seller,

            RegistrationDate = DateTimeOffset.UtcNow,

            AvatarUrl = "http://test.com/avatar.jpg",
            Biography = "Test Bio",
            RatingScore = 5.0
        };

        _createValidatorMock.Setup(v => v.ValidateAsync(model, Ct))
            .ReturnsAsync(new ValidationResult());

        _repositoryMock.Setup(r => r.GetByEmail(model.Email, Ct))
            .ReturnsAsync((User?)null);

        MapperMock.Setup(m => m.Map<User>(model)).Returns(userEntity);
        MapperMock.Setup(m => m.Map<UserModel>(userEntity)).Returns(expectedResult);

        var result = await _service.Create(model, Ct);

        result.ShouldBe(expectedResult);

        userEntity.Profile.ShouldNotBeNull();
        userEntity.Profile.RatingScore.ShouldBe(1);
        userEntity.PasswordHash.ShouldBe(model.Password);

        _repositoryMock.Verify(r => r.Add(userEntity, Ct), Times.Once);
    }

    [Fact]
    public async Task Create_ShouldSetDefaultRole_WhenRoleIsNone()
    {
        var model = new CreateUserModel
        {
            Email = "new@test.com",
            Role = UserRoles.None,
            Username = "TestUser",
            Password = "Password123"
        };

        var userEntity = CreateUser(role: UserRoles.None);

        _createValidatorMock.Setup(v => v.ValidateAsync(model, Ct))
            .ReturnsAsync(new ValidationResult());

        _repositoryMock.Setup(r => r.GetByEmail(model.Email, Ct))
            .ReturnsAsync((User?)null);

        MapperMock.Setup(m => m.Map<User>(model)).Returns(userEntity);

        MapperMock.Setup(m => m.Map<UserModel>(It.IsAny<User>()))
            .Returns(new UserModel
            {
                Id = userEntity.Id,
                Username = model.Username,
                Email = model.Email,
                Role = UserRolePresets.BuyerOnly
            });

        await _service.Create(model, Ct);

        userEntity.Role.ShouldBe(UserRolePresets.BuyerOnly);
    }

    [Fact]
    public async Task Create_ShouldThrowInvalidOperationException_WhenEmailExists()
    {
        var model = new CreateUserModel
        {
            Email = "exist@test.com",
            Username = "TestUser",
            Password = "Password123"
        };
        var existingUser = CreateUser(email: model.Email);

        _createValidatorMock.Setup(v => v.ValidateAsync(model, Ct))
            .ReturnsAsync(new ValidationResult());

        _repositoryMock.Setup(r => r.GetByEmail(model.Email, Ct))
            .ReturnsAsync(existingUser);

        var exception = await Should.ThrowAsync<InvalidOperationException>(() =>
            _service.Create(model, Ct));

        exception.Message.ShouldBe($"User with email {model.Email} already exists.");

        _repositoryMock.Verify(r => r.Add(It.IsAny<User>(), Ct), Times.Never);
    }

    [Fact]
    public async Task Create_ShouldThrowValidationException_WhenValidatorFails()
    {
        var model = new CreateUserModel
        {
            Username = "Dummy",
            Email = "Dummy",
            Password = "Dummy"
        };

        var validationResult = new ValidationException(new[] { new ValidationFailure("Email", "Error") });

        _createValidatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<IValidationContext>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(validationResult);

        var exception = await Should.ThrowAsync<ValidationException>(() =>
            _service.Create(model, Ct));

        exception.Errors.ShouldContain(e => e.PropertyName == "Email" && e.ErrorMessage == "Error");
    }

    [Fact]
    public async Task Update_ShouldUpdateUser_WhenUserExists()
    {
        var model = new UpdateUserModel
        {
            Id = Guid.NewGuid(),
            Email = "new@email.com",
            Username = "NewUsername",
            Password = "NewPassword"
        };

        var existingUser = CreateUser(id: model.Id, email: "old@email.com");

        _updateValidatorMock.Setup(v => v.ValidateAsync(model, Ct))
            .ReturnsAsync(new ValidationResult());

        _repositoryMock.Setup(r => r.GetById(model.Id, Ct, false))
            .ReturnsAsync(existingUser);

        _repositoryMock.Setup(r => r.GetByEmail(model.Email, Ct))
            .ReturnsAsync((User?)null);

        MapperMock.Setup(m => m.Map<UserModel>(existingUser))
            .Returns(new UserModel
            {
                Id = existingUser.Id,
                Username = model.Username,
                Email = model.Email,
                Role = existingUser.Role
            });

        await _service.Update(model, Ct);

        _repositoryMock.Verify(r => r.Update(existingUser, Ct), Times.Once);
    }

    [Fact]
    public async Task Update_ShouldThrowKeyNotFound_WhenUserDoesNotExist()
    {
        var model = new UpdateUserModel
        {
            Id = Guid.NewGuid(),
            Username = "Dummy",
            Email = "dummy@test.com",
            Password = "Dummy"
        };

        _updateValidatorMock.Setup(v => v.ValidateAsync(model, Ct))
            .ReturnsAsync(new ValidationResult());

        _repositoryMock.Setup(r => r.GetById(model.Id, Ct, false))
            .ReturnsAsync((User?)null);

        var exception = await Should.ThrowAsync<KeyNotFoundException>(() =>
            _service.Update(model, Ct));

        exception.Message.ShouldBe($"User {model.Id} not found");
    }

    [Fact]
    public async Task Delete_ShouldCallRepositoryDelete_WhenUserExists()
    {
        var id = Guid.NewGuid();
        var user = CreateUser(id: id);

        _repositoryMock.Setup(r => r.GetById(id, Ct, false))
            .ReturnsAsync(user);

        await _service.Delete(id, Ct);

        _repositoryMock.Verify(r => r.Delete(user, Ct), Times.Once);
    }

    [Fact]
    public async Task Delete_ShouldThrowKeyNotFound_WhenUserDoesNotExist()
    {
        var id = Guid.NewGuid();
        _repositoryMock.Setup(r => r.GetById(id, Ct, false))
            .ReturnsAsync((User?)null);

        var exception = await Should.ThrowAsync<KeyNotFoundException>(() =>
            _service.Delete(id, Ct));

        exception.Message.ShouldBe($"User {id} not found");
    }

    private static User CreateUser(
        Guid? id = null,
        string? email = null,
        UserRoles role = UserRoles.Buyer)
    {
        return new User
        {
            Id = id ?? Guid.NewGuid(),
            Username = $"User_{id ?? Guid.NewGuid()}",
            Email = email ?? $"user_{id ?? Guid.NewGuid()}@test.com",
            PasswordHash = "default_hash",
            Role = role,
            RegistrationDate = DateTimeOffset.UtcNow,

            Profile = null!,
            Products = null!
        };
    }
}
