using Microsoft.EntityFrameworkCore;
using ProductService.Domain.Constants;
using System.ComponentModel.DataAnnotations;

namespace ProductService.Domain.Entities;

[Index(nameof(Name), IsUnique = true)]
public class Category
{
    public required Guid Id { get; set; }

    [MaxLength(DbConstants.NameTextLength)]
    public required string Name { get; set; }

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}