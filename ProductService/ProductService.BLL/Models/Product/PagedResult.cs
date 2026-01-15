using System.Diagnostics.CodeAnalysis;

namespace ProductService.BLL.Models;

[ExcludeFromCodeCoverage]
public class PagedResult<T>
{
    public IList<T> Items { get; init; } = [];

    public string? ContinuationToken { get; init; }
}
