using Microsoft.EntityFrameworkCore;
using ProductService.Domain.Constants;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace ProductService.Domain.Entities;

[Index(nameof(Name), IsUnique = true)]
[ExcludeFromCodeCoverage]
public class Category : BaseEntity
{
    [MaxLength(DbConstants.NameTextLength)]
    public required string Name { get; set; }

    public virtual IEnumerable<Product> Products { get; set; } = new List<Product>();
}
