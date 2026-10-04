using System.Diagnostics.CodeAnalysis;

namespace ProductService.BLL.Models;

[ExcludeFromCodeCoverage]
public sealed record Caller(string ExternalId, bool IsAdmin);
