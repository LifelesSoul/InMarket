namespace ProductService.DAL.Models;

public class PagedList<T>
{
    public IReadOnlyList<T> Items { get; init; } = [];

    public Guid? LastId { get; init; }
}
