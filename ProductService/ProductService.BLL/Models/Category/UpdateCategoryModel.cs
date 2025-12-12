namespace ProductService.BLL.Models.Category;

public class UpdateCategoryModel
{
    public Guid Id { get; set; }

    public required string Name { get; set; }
}
