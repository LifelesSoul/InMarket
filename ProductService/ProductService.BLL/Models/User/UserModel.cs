namespace ProductService.BLL.Models.User;

public class UserModel : BaseModel
{
    public required string Username { get; set; }

    public required string Email { get; set; }

    public required string Role { get; set; }

    public DateTimeOffset RegistrationDate { get; set; }

    public string? AvatarUrl { get; set; }

    public string? Biography { get; set; }

    public double RatingScore { get; set; }
}
