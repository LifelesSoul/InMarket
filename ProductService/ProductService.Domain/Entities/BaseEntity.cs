using System.Diagnostics.CodeAnalysis;

namespace ProductService.Domain.Entities;

[ExcludeFromCodeCoverage]
public abstract class BaseEntity
{
    public Guid Id { get; set; }
}
