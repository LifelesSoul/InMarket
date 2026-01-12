namespace ProductService.BLL.Models;

public class PagedResult<T>
{
    public IList<T> Items { get; init; } = [];

    public string? ContinuationToken { get; init; }
}
