namespace UserService.Domain.Entities;

public class UserProfile
{
    public required Guid UserId { get; set; }

    public required virtual User User { get; set; }

    public string? AvatarUrl { get; set; }
    public string? Bio { get; set; }
    public double RatingScore { get; set; } = default;
}
