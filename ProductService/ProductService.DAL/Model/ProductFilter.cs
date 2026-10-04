using ProductService.Domain.Enums;
using System.Diagnostics.CodeAnalysis;

namespace ProductService.DAL.Models;

[ExcludeFromCodeCoverage]
public sealed record ProductFilter(Guid? SellerId = null, ProductStatus? Status = null);
