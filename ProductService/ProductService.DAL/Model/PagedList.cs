using System.Diagnostics.CodeAnalysis;

namespace ProductService.DAL.Models;

[ExcludeFromCodeCoverage]
public class PagedList<T>
{
    public IReadOnlyList<T> Items { get; init; } = [];

    public Guid? LastId { get; init; }
}
