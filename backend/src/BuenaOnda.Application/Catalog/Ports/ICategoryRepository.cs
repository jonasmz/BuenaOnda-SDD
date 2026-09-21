using BuenaOnda.Domain.Catalog;

namespace BuenaOnda.Application.Catalog.Ports;

/// <summary>Puerto de salida para la persistencia de categorías.</summary>
public interface ICategoryRepository
{
    Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Category>> ListAsync(bool includeInactive, CancellationToken cancellationToken = default);

    /// <summary>Indica si otra categoría (distinta de <paramref name="excludingId"/>) tiene ese nombre normalizado.</summary>
    Task<bool> ExistsWithNormalizedNameAsync(string normalizedName, Guid? excludingId, CancellationToken cancellationToken = default);

    Task AddAsync(Category category, CancellationToken cancellationToken = default);
}
