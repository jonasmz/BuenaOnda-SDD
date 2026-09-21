using BuenaOnda.Application.Catalog.Ports;
using BuenaOnda.Domain.Catalog;
using BuenaOnda.Domain.Common;

namespace BuenaOnda.Application.Catalog.Products;

public sealed record CharacteristicView(Guid Id, string Name);

public sealed record OptionView(
    Guid Id, IReadOnlyDictionary<string, string> Values, decimal Price, string? Description, string? ImageUrl,
    bool IsMarkedAvailable, bool IsActive, bool IsAvailable, bool IsVisibleToPublic);

public sealed record ProductView(
    Guid Id, string Name, string? Description, string? ImageUrl, bool HasImage, Guid CategoryId, bool IsActive,
    bool IsAvailable, bool IsVisibleToPublic, IReadOnlyList<CharacteristicView> Characteristics,
    IReadOnlyList<OptionView> Options)
{
    public static ProductView From(Product product, Category category) => new(
        product.Id, product.Name, product.Description, product.ImageUrl, product.HasImage, product.CategoryId,
        product.IsActive, product.IsAvailable(category), product.IsVisibleToPublic(category),
        product.Characteristics.Select(c => new CharacteristicView(c.Id, c.Name)).ToList(),
        product.Options.Select(o => new OptionView(
            o.Id, ValuesByName(product, o), o.Price, o.Description, o.ImageUrl, o.IsMarkedAvailable, o.IsActive,
            o.IsAvailable(product, category), o.IsVisibleToPublic(product, category))).ToList());

    private static Dictionary<string, string> ValuesByName(Product product, SellableOption option) =>
        product.Characteristics.ToDictionary(c => c.Name, c => option.Values.First(v => v.CharacteristicId == c.Id).Value);
}

public sealed record OptionInput(
    IReadOnlyDictionary<string, string>? Values, decimal? Price, bool? IsMarkedAvailable, string? Description, string? ImageUrl)
{
    internal OptionDraft ToDraft() => new(Values, Price, IsMarkedAvailable, Description, ImageUrl);
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
        string? name, string? description, string? imageUrl, Guid? categoryId,
        IReadOnlyList<string>? characteristics, IReadOnlyList<OptionInput>? options,
        CancellationToken cancellationToken = default)
    {
        var category = await CategoryLookup.RequireAsync(categories, categoryId, cancellationToken);
        var product = Product.Create(
            name, description, imageUrl, category, characteristics, options?.Select(o => o.ToDraft()).ToList());
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

/// <summary>Base de los casos de uso que modifican la estructura de un producto ya existente.</summary>
public abstract class ProductStructureUseCase(IProductRepository products, ICategoryRepository categories, IUnitOfWork unitOfWork)
{
    protected Task<ProductView> ApplyAsync(
        Guid productId, Action<Product, Category> change, CancellationToken cancellationToken) =>
        ApplyAsync(productId, (product, category) =>
        {
            change(product, category);
            return Task.CompletedTask;
        }, cancellationToken);

    protected async Task<ProductView> ApplyAsync(
        Guid productId, Func<Product, Category, Task> change, CancellationToken cancellationToken)
    {
        var product = await products.GetByIdAsync(productId, cancellationToken)
            ?? throw new NotFoundException($"No existe el producto {productId}.");
        var category = await categories.GetByIdAsync(product.CategoryId, cancellationToken)
            ?? throw new NotFoundException($"No existe la categoría {product.CategoryId}.");

        await change(product, category);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return ProductView.From(product, category);
    }
}

public sealed class AddOption(IProductRepository products, ICategoryRepository categories, IUnitOfWork unitOfWork)
    : ProductStructureUseCase(products, categories, unitOfWork)
{
    public Task<ProductView> ExecuteAsync(Guid productId, OptionInput input, CancellationToken cancellationToken = default) =>
        ApplyAsync(productId, (product, category) => product.AddOption(input.ToDraft(), category), cancellationToken);
}

public sealed class AddCharacteristic(IProductRepository products, ICategoryRepository categories, IUnitOfWork unitOfWork)
    : ProductStructureUseCase(products, categories, unitOfWork)
{
    public Task<ProductView> ExecuteAsync(
        Guid productId, string? name, IReadOnlyDictionary<Guid, string>? valuesForExistingOptions,
        CancellationToken cancellationToken = default) =>
        ApplyAsync(
            productId, (product, category) => product.AddCharacteristic(name, valuesForExistingOptions, category), cancellationToken);
}

public sealed class RemoveCharacteristic(IProductRepository products, ICategoryRepository categories, IUnitOfWork unitOfWork)
    : ProductStructureUseCase(products, categories, unitOfWork)
{
    public Task<ProductView> ExecuteAsync(Guid productId, Guid characteristicId, CancellationToken cancellationToken = default) =>
        ApplyAsync(productId, (product, category) => product.RemoveCharacteristic(characteristicId, category), cancellationToken);
}

public sealed class DeactivateProduct(IProductRepository products, ICategoryRepository categories, IUnitOfWork unitOfWork)
    : ProductStructureUseCase(products, categories, unitOfWork)
{
    public Task<ProductView> ExecuteAsync(Guid productId, CancellationToken cancellationToken = default) =>
        ApplyAsync(productId, (product, _) => product.Deactivate(), cancellationToken);
}

public sealed class ReactivateProduct(IProductRepository products, ICategoryRepository categories, IUnitOfWork unitOfWork)
    : ProductStructureUseCase(products, categories, unitOfWork)
{
    public Task<ProductView> ExecuteAsync(Guid productId, CancellationToken cancellationToken = default) =>
        ApplyAsync(productId, (product, _) => product.Reactivate(), cancellationToken);
}

public sealed class UpdateOption(IProductRepository products, ICategoryRepository categories, IUnitOfWork unitOfWork)
    : ProductStructureUseCase(products, categories, unitOfWork)
{
    public Task<ProductView> ExecuteAsync(
        Guid productId, Guid optionId, OptionInput input, CancellationToken cancellationToken = default) =>
        ApplyAsync(productId, (product, category) => product.UpdateOption(optionId, input.ToDraft(), category), cancellationToken);
}

public sealed class SetOptionAvailability(IProductRepository products, ICategoryRepository categories, IUnitOfWork unitOfWork)
    : ProductStructureUseCase(products, categories, unitOfWork)
{
    public Task<ProductView> ExecuteAsync(
        Guid productId, Guid optionId, bool? isMarkedAvailable, CancellationToken cancellationToken = default) =>
        ApplyAsync(
            productId, (product, category) => product.SetOptionAvailability(optionId, isMarkedAvailable, category), cancellationToken);
}

public sealed class DeactivateOption(IProductRepository products, ICategoryRepository categories, IUnitOfWork unitOfWork)
    : ProductStructureUseCase(products, categories, unitOfWork)
{
    public Task<ProductView> ExecuteAsync(Guid productId, Guid optionId, CancellationToken cancellationToken = default) =>
        ApplyAsync(productId, (product, category) => product.DeactivateOption(optionId, category), cancellationToken);
}

public sealed class ReactivateOption(IProductRepository products, ICategoryRepository categories, IUnitOfWork unitOfWork)
    : ProductStructureUseCase(products, categories, unitOfWork)
{
    public Task<ProductView> ExecuteAsync(Guid productId, Guid optionId, CancellationToken cancellationToken = default) =>
        ApplyAsync(productId, (product, category) => product.ReactivateOption(optionId, category), cancellationToken);
}

public sealed class DeleteOption(
    IProductRepository products, ICategoryRepository categories, IUnitOfWork unitOfWork, IOptionReferenceChecker references)
    : ProductStructureUseCase(products, categories, unitOfWork)
{
    public Task<ProductView> ExecuteAsync(Guid productId, Guid optionId, CancellationToken cancellationToken = default) =>
        ApplyAsync(productId, async (product, category) =>
        {
            var referenced = await references.IsReferencedAsync(optionId, cancellationToken);
            product.RemoveOption(optionId, referenced, category);
        }, cancellationToken);
}
