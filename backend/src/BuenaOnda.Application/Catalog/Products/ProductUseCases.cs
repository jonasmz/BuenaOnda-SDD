using BuenaOnda.Application.Catalog.Ports;
using BuenaOnda.Domain.Catalog;
using BuenaOnda.Domain.Common;

namespace BuenaOnda.Application.Catalog.Products;

public sealed record ProductView(
    Guid Id, string Name, string? Description, string? ImageUrl, bool HasImage, Guid CategoryId, decimal Price,
    bool IsActive, bool IsMarkedAvailable, bool IsAvailable, bool IsVisibleToPublic)
{
    public static ProductView From(Product product, Category category) => new(
        product.Id, product.Name, product.Description, product.ImageUrl, product.HasImage, product.CategoryId,
        product.Price, product.IsActive, product.IsMarkedAvailable, product.IsAvailable(category),
        product.IsVisibleToPublic(category));
}

internal static class CategoryLookup
{
    public static async Task<Category> RequireAsync(ICategoryRepository categories, Guid? id, CancellationToken ct)
    {
        if (id is null)
        {
            throw new ValidationException("La categoría es obligatoria.");
        }

        return await categories.GetByIdAsync(id.Value, ct)
            ?? throw new NotFoundException($"No existe la categoría {id}.");
    }
}

public sealed class CreateProduct(IProductRepository products, ICategoryRepository categories, IUnitOfWork unitOfWork)
{
    public async Task<ProductView> ExecuteAsync(
        string? name, string? description, string? imageUrl, Guid? categoryId, decimal? price, bool? isMarkedAvailable,
        CancellationToken cancellationToken = default)
    {
        var category = await CategoryLookup.RequireAsync(categories, categoryId, cancellationToken);
        var product = Product.Create(name, description, imageUrl, category, price, isMarkedAvailable);
        if (await products.ExistsWithNormalizedNameInCategoryAsync(product.NormalizedName, category.Id, null, cancellationToken))
        {
            throw new ConflictException($"Ya existe un producto llamado '{product.Name}' en la categoría.");
        }

        await products.AddAsync(product, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return ProductView.From(product, category);
    }
}

public sealed class UpdateProduct(IProductRepository products, ICategoryRepository categories, IUnitOfWork unitOfWork)
{
    public async Task<ProductView> ExecuteAsync(
        Guid id, string? name, string? description, string? imageUrl, Guid? categoryId, decimal? price,
        CancellationToken cancellationToken = default)
    {
        var product = await products.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"No existe el producto {id}.");
        var current = await categories.GetByIdAsync(product.CategoryId, cancellationToken)
            ?? throw new NotFoundException($"No existe la categoría {product.CategoryId}.");
        var target = categoryId is null || categoryId == current.Id
            ? current
            : await CategoryLookup.RequireAsync(categories, categoryId, cancellationToken);

        product.Update(name, description, imageUrl, price, current, target);
        if (await products.ExistsWithNormalizedNameInCategoryAsync(product.NormalizedName, target.Id, product.Id, cancellationToken))
        {
            throw new ConflictException($"Ya existe un producto llamado '{product.Name}' en la categoría.");
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return ProductView.From(product, target);
    }
}

public sealed class GetProduct(IProductRepository products, ICategoryRepository categories)
{
    public async Task<ProductView> ExecuteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var product = await products.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"No existe el producto {id}.");
        var category = await categories.GetByIdAsync(product.CategoryId, cancellationToken)
            ?? throw new NotFoundException($"No existe la categoría {product.CategoryId}.");
        return ProductView.From(product, category);
    }
}

public sealed class ListProducts(IProductRepository products, ICategoryRepository categories)
{
    public async Task<IReadOnlyList<ProductView>> ExecuteAsync(
        Guid? categoryId, bool includeInactive, CancellationToken cancellationToken = default)
    {
        var byId = (await categories.ListAsync(true, cancellationToken)).ToDictionary(c => c.Id);
        return (await products.ListAsync(categoryId, includeInactive, cancellationToken))
            .Select(p => ProductView.From(p, byId[p.CategoryId]))
            .ToList();
    }
}

/// <summary>Base de los casos de uso que cambian el estado de un producto ya existente.</summary>
public abstract class ProductChangeUseCase(IProductRepository products, ICategoryRepository categories, IUnitOfWork unitOfWork)
{
    protected async Task<ProductView> ApplyAsync(
        Guid productId, Action<Product, Category> change, CancellationToken cancellationToken)
    {
        var product = await products.GetByIdAsync(productId, cancellationToken)
            ?? throw new NotFoundException($"No existe el producto {productId}.");
        var category = await categories.GetByIdAsync(product.CategoryId, cancellationToken)
            ?? throw new NotFoundException($"No existe la categoría {product.CategoryId}.");

        change(product, category);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return ProductView.From(product, category);
    }
}

public sealed class SetProductAvailability(IProductRepository products, ICategoryRepository categories, IUnitOfWork unitOfWork)
    : ProductChangeUseCase(products, categories, unitOfWork)
{
    public Task<ProductView> ExecuteAsync(Guid productId, bool? isMarkedAvailable, CancellationToken cancellationToken = default) =>
        ApplyAsync(productId, (product, category) => product.SetAvailability(isMarkedAvailable, category), cancellationToken);
}

public sealed class DeactivateProduct(IProductRepository products, ICategoryRepository categories, IUnitOfWork unitOfWork)
    : ProductChangeUseCase(products, categories, unitOfWork)
{
    public Task<ProductView> ExecuteAsync(Guid productId, CancellationToken cancellationToken = default) =>
        ApplyAsync(productId, (product, _) => product.Deactivate(), cancellationToken);
}

public sealed class ReactivateProduct(IProductRepository products, ICategoryRepository categories, IUnitOfWork unitOfWork)
    : ProductChangeUseCase(products, categories, unitOfWork)
{
    public Task<ProductView> ExecuteAsync(Guid productId, CancellationToken cancellationToken = default) =>
        ApplyAsync(productId, (product, _) => product.Reactivate(), cancellationToken);
}
