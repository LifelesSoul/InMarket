using System.Diagnostics.CodeAnalysis;

namespace ProductService.BLL.Models.Product;

[ExcludeFromCodeCoverage]
public class ProductCategoryModel : BaseModel
{
    public required string Name { get; set; }
}
