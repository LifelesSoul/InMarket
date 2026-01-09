namespace ProductService.BLL.Models.User;

public class UpdateUserModel : BaseModel
{
    public string? Biography { get; set; }

    public string? AvatarUrl { get; set; }

    public required string Username { get; set; }

    public required string Email { get; set; }

    public required string Password { get; set; }
}
