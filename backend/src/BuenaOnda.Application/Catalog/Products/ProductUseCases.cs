using BuenaOnda.Application.Catalog.Ports;
using BuenaOnda.Domain.Catalog;
using BuenaOnda.Domain.Common;

namespace BuenaOnda.Application.Catalog.Products;

public sealed record OptionView(
    Guid Id, decimal Price, string? Description, string? ImageUrl,
    bool IsMarkedAvailable, bool IsActive, bool IsAvailable, bool IsVisibleToPublic);

public sealed record ProductView(
    Guid Id, string Name, string? Description, string? ImageUrl, bool HasImage, Guid CategoryId, bool IsActive,
    bool IsAvailable, bool IsVisibleToPublic, IReadOnlyList<OptionView> Options)
{
    public static ProductView From(Product product, Category category) => new(
        product.Id, product.Name, product.Description, product.ImageUrl, product.HasImage, product.CategoryId,
        product.IsActive, product.IsAvailable(category), product.IsVisibleToPublic(category),
        product.Options.Select(o => new OptionView(
            o.Id, o.Price, o.Description, o.ImageUrl, o.IsMarkedAvailable, o.IsActive,
            o.IsAvailable(product, category), o.IsVisibleToPublic(product, category))).ToList());
}

public sealed record OptionInput(decimal? Price, bool? IsMarkedAvailable, string? Description, string? ImageUrl);

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
        string? name, string? description, string? imageUrl, Guid? categoryId, OptionInput? option,
        CancellationToken cancellationToken = default)
    {
        var category = await CategoryLookup.RequireAsync(categories, categoryId, cancellationToken);
        if (option is null)
        {
            throw new ValidationException("El producto necesita una opción con su precio.");
        }

        var product = Product.Create(
            name, description, imageUrl, category, option.Price, option.IsMarkedAvailable, option.Description, option.ImageUrl);
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
        Guid id, string? name, string? description, string? imageUrl, Guid? categoryId,
        CancellationToken cancellationToken = default)
    {
        var product = await products.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"No existe el producto {id}.");
        var current = await categories.GetByIdAsync(product.CategoryId, cancellationToken)
            ?? throw new NotFoundException($"No existe la categoría {product.CategoryId}.");
        var target = categoryId is null || categoryId == current.Id
            ? current
            : await CategoryLookup.RequireAsync(categories, categoryId, cancellationToken);

        product.Update(name, description, imageUrl, current, target);
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
