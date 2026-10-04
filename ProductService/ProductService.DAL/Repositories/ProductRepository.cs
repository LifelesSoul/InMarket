using Microsoft.EntityFrameworkCore;
using ProductService.DAL.Models;
using ProductService.Domain.Entities;
using ProductService.Infrastructure;
using System.Diagnostics.CodeAnalysis;

namespace ProductService.DAL.Repositories;

[ExcludeFromCodeCoverage]
public class ProductRepository(ProductDbContext context) : Repository<Product>(context), IProductRepository
{
    public override async Task<Product?> GetById(Guid id, CancellationToken cancellationToken, bool disableTracking = false)
    {
        if (!disableTracking)
        {
            return await base.GetById(id, cancellationToken, disableTracking);
        }

        return await WithDetails(DbSet.AsNoTracking())
            .FirstOrDefaultAsync(product => product.Id == id, cancellationToken);
    }

    public async Task<PagedList<Product>> GetPaged(
        int limit,
        Guid? lastId,
        ProductFilter filter,
        CancellationToken cancellationToken)
    {
        var query = WithDetails(DbSet.AsNoTracking());

        if (filter.SellerId.HasValue)
        {
            query = query.Where(product => product.SellerId == filter.SellerId.Value);
        }

        if (filter.Status.HasValue)
        {
            query = query.Where(product => product.Status == filter.Status.Value);
        }

        if (lastId.HasValue)
        {
            query = query.Where(product => product.Id > lastId.Value);
        }

        var items = await query
            .OrderBy(product => product.Id)
            .Take(limit)
            .ToListAsync(cancellationToken);

        return new PagedList<Product>
        {
            Items = items,
            LastId = items.Count > 0 ? items[^1].Id : null
        };
    }

    public override async Task<Product> Add(Product product, CancellationToken cancellationToken)
    {
        var addedProduct = await base.Add(product, cancellationToken);

        await Context.Entry(addedProduct).Reference(p => p.Category).LoadAsync(cancellationToken);
        await Context.Entry(addedProduct).Reference(p => p.Seller).LoadAsync(cancellationToken);

        return addedProduct;
    }

    public async Task Update(Product product, IEnumerable<string>? newImageUrls, CancellationToken cancellationToken)
    {
        if (newImageUrls is not null)
        {
            var incomingUrls = newImageUrls.ToList();

            var currentImages = await Context.Set<ProductImage>()
                .Where(p => p.ProductId == product.Id)
                .ToListAsync(cancellationToken);

            var imagesToDelete = new List<ProductImage>();

            foreach (var dbImage in currentImages)
            {
                var index = incomingUrls.IndexOf(dbImage.Url);

                if (index is not -1)
                {
                    incomingUrls.RemoveAt(index);
                }
                else
                {
                    imagesToDelete.Add(dbImage);
                }
            }

            if (imagesToDelete.Count > 0)
            {
                Context.Set<ProductImage>().RemoveRange(imagesToDelete);
            }

            if (incomingUrls.Count > 0)
            {
                var imagesToAdd = incomingUrls.Select(url => new ProductImage
                {
                    ProductId = product.Id,
                    Url = url
                }).ToList();

                await Context.Set<ProductImage>().AddRangeAsync(imagesToAdd, cancellationToken);
            }
        }
        await base.Update(product, cancellationToken);
    }

    private static IQueryable<Product> WithDetails(IQueryable<Product> query)
    {
        return query
            .Include(product => product.Category)
            .Include(product => product.Seller)
            .Include(product => product.Images);
    }
}

public interface IProductRepository : IRepository<Product>
{
    Task<PagedList<Product>> GetPaged(int limit, Guid? lastId, ProductFilter filter, CancellationToken cancellationToken);
    Task Update(Product product, IEnumerable<string>? newImageUrls, CancellationToken cancellationToken);
}
