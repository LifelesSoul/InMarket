using System.Diagnostics.CodeAnalysis;

namespace ProductService.Domain.Constants;

[ExcludeFromCodeCoverage]
public static class PaginationConstants
{
    public const int DefaultPageNumber = 1;
    public const int DefaultPageSize = 10;
}
