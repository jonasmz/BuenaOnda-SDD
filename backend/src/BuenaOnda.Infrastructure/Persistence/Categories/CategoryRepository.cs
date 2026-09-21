using BuenaOnda.Application.Catalog.Ports;
using BuenaOnda.Domain.Catalog;
using Microsoft.EntityFrameworkCore;

namespace BuenaOnda.Infrastructure.Persistence.Categories;

internal sealed class CategoryRepository(CatalogDbContext dbContext) : ICategoryRepository
{
    public Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.Set<Category>().FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Category>> ListAsync(bool includeInactive, CancellationToken cancellationToken = default) =>
        await dbContext.Set<Category>()
            .AsNoTracking()
            .Where(c => includeInactive || c.IsActive)
            .OrderBy(c => c.NormalizedName)
            .ToListAsync(cancellationToken);

    public Task<bool> ExistsWithNormalizedNameAsync(string normalizedName, Guid? excludingId, CancellationToken cancellationToken = default) =>
        dbContext.Set<Category>()
            .AnyAsync(c => c.NormalizedName == normalizedName && (excludingId == null || c.Id != excludingId), cancellationToken);

    public async Task AddAsync(Category category, CancellationToken cancellationToken = default) =>
        await dbContext.Set<Category>().AddAsync(category, cancellationToken);
}
