using BuenaOnda.Domain.Catalog;

namespace BuenaOnda.Application.Catalog.Ports;

/// <summary>Puerto de salida para la persistencia de productos con sus opciones.</summary>
public interface IProductRepository
{
    Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Product>> ListAsync(Guid? categoryId, bool includeInactive, CancellationToken cancellationToken = default);

    /// <summary>Indica si otro producto (distinto de <paramref name="excludingId"/>) tiene ese nombre normalizado en la categoría.</summary>
    Task<bool> ExistsWithNormalizedNameInCategoryAsync(
        string normalizedName, Guid categoryId, Guid? excludingId, CancellationToken cancellationToken = default);

    Task AddAsync(Product product, CancellationToken cancellationToken = default);
}
