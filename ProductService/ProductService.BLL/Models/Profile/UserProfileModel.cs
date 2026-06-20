namespace ProductService.BLL.Models.Profile;

public record UserProfileDto
{
    public required string Username { get; init; }

    public required string Email { get; init; }

    public required string AvatarUrl { get; init; }

    public required string Biography { get; init; }

    public required double RatingScore { get; init; }
};
