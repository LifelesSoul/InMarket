using System.Diagnostics.CodeAnalysis;

namespace ProductService.BLL.Models.Profile;

[ExcludeFromCodeCoverage]
public record UserProfileDto
{
    public required Guid Id { get; init; }

    public required string Username { get; init; }

    public required string Email { get; init; }

    public required string AvatarUrl { get; init; }

    public required string Biography { get; init; }

    public required double RatingScore { get; init; }

    public required DateTimeOffset RegistrationDate { get; init; }

    public required IReadOnlyList<string> Roles { get; init; }
};
