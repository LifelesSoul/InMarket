using Microsoft.EntityFrameworkCore;
using ProductService.Domain.Constants;
using System.ComponentModel.DataAnnotations;

namespace ProductService.Domain.Entities;

[Index(nameof(Name), IsUnique = true)]
public class Category : BaseEntity
{
    [MaxLength(DbConstants.NameTextLength)]
    public required string Name { get; set; }

    public virtual IEnumerable<Product> Products { get; set; } = new List<Product>();
}
