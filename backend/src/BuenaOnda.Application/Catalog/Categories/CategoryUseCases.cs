using BuenaOnda.Application.Catalog.Ports;
using BuenaOnda.Domain.Catalog;
using BuenaOnda.Domain.Common;

namespace BuenaOnda.Application.Catalog.Categories;

public sealed record CategoryView(Guid Id, string Name, string? Description, bool IsActive)
{
    public static CategoryView From(Category category) =>
        new(category.Id, category.Name, category.Description, category.IsActive);
}

public sealed class CreateCategory(ICategoryRepository categories, IUnitOfWork unitOfWork)
{
    public async Task<CategoryView> ExecuteAsync(string? name, string? description, CancellationToken cancellationToken = default)
    {
        var category = Category.Create(name, description);
        if (await categories.ExistsWithNormalizedNameAsync(category.NormalizedName, null, cancellationToken))
        {
            throw new ConflictException($"Ya existe una categoría llamada '{category.Name}'.");
        }

        await categories.AddAsync(category, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return CategoryView.From(category);
    }
}

public sealed class UpdateCategory(ICategoryRepository categories, IUnitOfWork unitOfWork)
{
    public async Task<CategoryView> ExecuteAsync(Guid id, string? name, string? description, CancellationToken cancellationToken = default)
    {
        var category = await categories.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"No existe la categoría {id}.");

        category.Update(name, description);
        if (await categories.ExistsWithNormalizedNameAsync(category.NormalizedName, category.Id, cancellationToken))
        {
            throw new ConflictException($"Ya existe una categoría llamada '{category.Name}'.");
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return CategoryView.From(category);
    }
}

public sealed class GetCategory(ICategoryRepository categories)
{
    public async Task<CategoryView> ExecuteAsync(Guid id, CancellationToken cancellationToken = default) =>
        CategoryView.From(await categories.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"No existe la categoría {id}."));
}

public sealed class ListCategories(ICategoryRepository categories)
{
    public async Task<IReadOnlyList<CategoryView>> ExecuteAsync(bool includeInactive, CancellationToken cancellationToken = default) =>
        (await categories.ListAsync(includeInactive, cancellationToken)).Select(CategoryView.From).ToList();
}
