namespace ProductService.BLL.Models.Product;
public class SellerModel
{
    public Guid Id { get; set; }

    public required string Username { get; set; }

    public required string Email { get; set; }
}
