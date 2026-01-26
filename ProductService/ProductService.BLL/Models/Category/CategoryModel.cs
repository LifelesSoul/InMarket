using System.Diagnostics.CodeAnalysis;

namespace ProductService.BLL.Models.Category;

[ExcludeFromCodeCoverage]
public class CategoryModel : BaseModel
{
    public required string Name { get; set; }
}
