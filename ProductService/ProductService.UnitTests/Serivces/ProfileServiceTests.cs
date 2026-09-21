using AutoMapper;
using Moq;
using ProductService.BLL.Models.Profile;
using ProductService.BLL.Services;
using ProductService.DAL.Repositories;
using Shouldly;
using UserService.Domain.Entities;
using UserService.Domain.Enums;
using Xunit;

namespace ProductService.Tests.Services.Profile;

public class ProfileServiceTests : ServiceTestsBase
{
    private readonly Mock<IProfileRepository> _repositoryMock;
    private readonly ProfileService _service;

    public ProfileServiceTests()
    {
        _repositoryMock = new Mock<IProfileRepository>();

        _service = new ProfileService(_repositoryMock.Object, MapperMock.Object);
    }

    [Fact]
    public async Task GetProfileByExternalIdAsync_WhenUserExists_ShouldReturnMappedProfile()
    {
        const string externalId = "auth0|abc123";

        var entity = CreateUser(externalId);

        entity.Profile = new UserProfile
        {
            UserId = entity.Id,
            User = entity,
            AvatarUrl = "https://example.com/avatar.png",
            Biography = "Sells things",
            RatingScore = 4.5
        };

        var expected = new UserProfileDto
        {
            Username = entity.Username,
            Email = entity.Email,
            AvatarUrl = "https://example.com/avatar.png",
            Biography = "Sells things",
            RatingScore = 4.5
        };

        _repositoryMock
            .Setup(repository => repository.GetUserWithProfileAsync(externalId))
            .ReturnsAsync(entity);

        MapperMock
            .Setup(mapper => mapper.Map<UserProfileDto>(entity))
            .Returns(expected);

        var result = await _service.GetProfileByExternalIdAsync(externalId);

        result.ShouldBe(expected);
        _repositoryMock.Verify(repository => repository.GetUserWithProfileAsync(externalId), Times.Once);
        MapperMock.Verify(mapper => mapper.Map<UserProfileDto>(entity), Times.Once);
    }

    [Fact]
    public async Task GetProfileByExternalIdAsync_WhenUserNotFound_ShouldReturnNull()
    {
        const string externalId = "auth0|missing";

        _repositoryMock
            .Setup(repository => repository.GetUserWithProfileAsync(externalId))
            .ReturnsAsync((User?)null);

        var result = await _service.GetProfileByExternalIdAsync(externalId);

        result.ShouldBeNull();
        _repositoryMock.Verify(repository => repository.GetUserWithProfileAsync(externalId), Times.Once);
        MapperMock.Verify(mapper => mapper.Map<UserProfileDto>(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task GetProfileByExternalIdAsync_ShouldQueryRepositoryWithGivenExternalId()
    {
        const string externalId = "auth0|exact-match";

        _repositoryMock
            .Setup(repository => repository.GetUserWithProfileAsync(It.IsAny<string>()))
            .ReturnsAsync((User?)null);

        await _service.GetProfileByExternalIdAsync(externalId);

        _repositoryMock.Verify(
            repository => repository.GetUserWithProfileAsync(
                It.Is<string>(value => value == externalId)),
            Times.Once);
    }

    private static User CreateUser(string externalId)
    {
        var id = Guid.NewGuid();

        return new User
        {
            Id = id,
            Username = $"User_{id}",
            Email = $"user_{id}@test.com",
            ExternalId = externalId,
            Role = UserRoles.Seller,
            RegistrationDate = DateTimeOffset.UtcNow,

            Profile = null!,
            Products = null!
        };
    }
}
