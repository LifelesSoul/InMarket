namespace ProductService.BLL.Models.User;
public class SellerViewModel
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTimeOffset RegistrationDate { get; set; }
}
