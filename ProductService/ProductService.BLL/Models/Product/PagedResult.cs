namespace ProductService.BLL.Models;

public class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; init; } = [];

    public string? ContinuationToken { get; init; }
}
