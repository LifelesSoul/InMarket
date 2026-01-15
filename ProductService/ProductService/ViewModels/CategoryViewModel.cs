using System.Diagnostics.CodeAnalysis;

namespace ProductService.API.Models;

[ExcludeFromCodeCoverage]
public class CategoryViewModel
{
    public Guid Id { get; set; }

    public required string Name { get; set; }
}
