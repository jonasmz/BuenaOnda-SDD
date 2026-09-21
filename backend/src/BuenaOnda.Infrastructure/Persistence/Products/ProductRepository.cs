using BuenaOnda.Application.Catalog.Ports;
using BuenaOnda.Domain.Catalog;
using Microsoft.EntityFrameworkCore;

namespace BuenaOnda.Infrastructure.Persistence.Products;

internal sealed class ProductRepository(CatalogDbContext dbContext) : IProductRepository
{
    public Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.Set<Product>().Include(p => p.Options).FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Product>> ListAsync(Guid? categoryId, bool includeInactive, CancellationToken cancellationToken = default) =>
        await dbContext.Set<Product>()
            .AsNoTracking()
            .Include(p => p.Options)
            .Where(p => (categoryId == null || p.CategoryId == categoryId) && (includeInactive || p.IsActive))
            .OrderBy(p => p.NormalizedName)
            .ToListAsync(cancellationToken);

    public Task<bool> ExistsWithNormalizedNameInCategoryAsync(
        string normalizedName, Guid categoryId, Guid? excludingId, CancellationToken cancellationToken = default) =>
        dbContext.Set<Product>().AnyAsync(
            p => p.CategoryId == categoryId && p.NormalizedName == normalizedName && (excludingId == null || p.Id != excludingId),
            cancellationToken);

    public async Task AddAsync(Product product, CancellationToken cancellationToken = default) =>
        await dbContext.Set<Product>().AddAsync(product, cancellationToken);
}
