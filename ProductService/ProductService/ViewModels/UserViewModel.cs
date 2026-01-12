namespace ProductService.API.Models;

public class UserViewModel
{
    public Guid Id { get; set; }

    public required string Username { get; set; }

    public required string Email { get; set; }

    public required string Role { get; set; }

    public string? AvatarUrl { get; set; }

    public string? Biography { get; set; }

    public double RatingScore { get; set; }

    public DateTimeOffset RegistrationDate { get; set; }
}
