using System.Diagnostics.CodeAnalysis;

namespace ProductService.BLL.Models;

[ExcludeFromCodeCoverage]
public abstract class BaseModel
{
    public Guid Id { get; set; }
}

