using System.Diagnostics.CodeAnalysis;

namespace ProductService.BLL.Exceptions;

[ExcludeFromCodeCoverage]
public sealed class ForbiddenException(string message) : Exception(message);
