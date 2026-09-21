using BuenaOnda.Application.Catalog.Categories;
using BuenaOnda.Application.Catalog.Ports;
using BuenaOnda.Application.Catalog.Products;
using BuenaOnda.Domain.Catalog;
using BuenaOnda.Domain.Common;

namespace BuenaOnda.Application.Tests.Catalog;

public class ProductUseCasesTests
{
    private readonly Repos _repos = new();
    private readonly Category _bebidas = Category.Create("Bebidas", null);
    private readonly Category _postres = Category.Create("Postres", null);
    private static readonly OptionInput Option = new(null, 1000m, true, null, null);

    public ProductUseCasesTests()
    {
        _repos.Categories.AddRange([_bebidas, _postres]);
    }

    private CreateProduct Create => new(_repos, _repos, _repos);

    [Fact]
    public async Task Create_saves_product_with_single_option_and_derived_indicators()
    {
        var view = await Create.ExecuteAsync("Agua", null, null, _bebidas.Id, null, [Option]);

        Assert.Single(view.Options);
        Assert.True(view.IsAvailable);
        Assert.False(view.IsVisibleToPublic);
        Assert.Equal(1, _repos.Saves);
    }

    [Fact]
    public async Task Same_name_in_same_category_is_conflict_but_allowed_in_another()
    {
        await Create.ExecuteAsync("Agua", null, null, _bebidas.Id, null, [Option]);

        await Assert.ThrowsAsync<ConflictException>(() => Create.ExecuteAsync(" AGUA ", null, null, _bebidas.Id, null, [Option]));
        await Create.ExecuteAsync("Agua", null, null, _postres.Id, null, [Option]);
    }

    [Fact]
    public async Task Create_requires_existing_active_category_and_option()
    {
        await Assert.ThrowsAsync<ValidationException>(() => Create.ExecuteAsync("X", null, null, null, null, [Option]));
        await Assert.ThrowsAsync<NotFoundException>(() => Create.ExecuteAsync("X", null, null, Guid.NewGuid(), null, [Option]));
        await Assert.ThrowsAsync<ValidationException>(() => Create.ExecuteAsync("X", null, null, _bebidas.Id, null, null));

        var inactive = Category.Create("Vieja", null);
        typeof(Category).GetProperty(nameof(Category.IsActive))!.SetValue(inactive, false);
        _repos.Categories.Add(inactive);
        await Assert.ThrowsAsync<ConflictException>(() => Create.ExecuteAsync("X", null, null, inactive.Id, null, [Option]));
    }

    [Fact]
    public async Task Update_moving_to_category_with_same_name_is_conflict()
    {
        var agua = await Create.ExecuteAsync("Agua", null, null, _bebidas.Id, null, [Option]);
        await Create.ExecuteAsync("Agua", null, null, _postres.Id, null, [Option]);

        await Assert.ThrowsAsync<ConflictException>(() =>
            new UpdateProduct(_repos, _repos, _repos).ExecuteAsync(agua.Id, "Agua", null, null, _postres.Id));
    }

    [Fact]
    public async Task Update_changes_information_and_allows_keeping_own_name()
    {
        var agua = await Create.ExecuteAsync("Agua", null, null, _bebidas.Id, null, [Option]);

        var updated = await new UpdateProduct(_repos, _repos, _repos)
            .ExecuteAsync(agua.Id, "AGUA", "Fría", "https://cdn.example.com/a.png", null);

        Assert.Equal("AGUA", updated.Name);
        Assert.True(updated.IsVisibleToPublic);
        Assert.Equal(_bebidas.Id, updated.CategoryId);
    }

    [Fact]
    public async Task Get_and_list_filter_by_category()
    {
        var agua = await Create.ExecuteAsync("Agua", null, null, _bebidas.Id, null, [Option]);
        await Create.ExecuteAsync("Flan", null, null, _postres.Id, null, [Option]);

        Assert.Equal(agua.Id, (await new GetProduct(_repos, _repos).ExecuteAsync(agua.Id)).Id);
        Assert.Single(await new ListProducts(_repos, _repos).ExecuteAsync(_bebidas.Id, false));
        Assert.Equal(2, (await new ListProducts(_repos, _repos).ExecuteAsync(null, false)).Count);
        await Assert.ThrowsAsync<NotFoundException>(() => new GetProduct(_repos, _repos).ExecuteAsync(Guid.NewGuid()));
    }

    private sealed class Repos : IProductRepository, ICategoryRepository, IUnitOfWork
    {
        public List<Category> Categories { get; } = [];
        public List<Product> Products { get; } = [];
        public int Saves { get; private set; }

        Task<Category?> ICategoryRepository.GetByIdAsync(Guid id, CancellationToken ct) => Task.FromResult(Categories.FirstOrDefault(c => c.Id == id));
        Task<IReadOnlyList<Category>> ICategoryRepository.ListAsync(bool includeInactive, CancellationToken ct) =>
            Task.FromResult<IReadOnlyList<Category>>(Categories.ToList());
        Task<bool> ICategoryRepository.ExistsWithNormalizedNameAsync(string n, Guid? e, CancellationToken ct) => Task.FromResult(false);
        Task ICategoryRepository.AddAsync(Category category, CancellationToken ct) { Categories.Add(category); return Task.CompletedTask; }

        Task<Product?> IProductRepository.GetByIdAsync(Guid id, CancellationToken ct) => Task.FromResult(Products.FirstOrDefault(p => p.Id == id));
        Task<IReadOnlyList<Product>> IProductRepository.ListAsync(Guid? categoryId, bool includeInactive, CancellationToken ct) =>
            Task.FromResult<IReadOnlyList<Product>>(Products.Where(p => categoryId == null || p.CategoryId == categoryId).ToList());
        Task<bool> IProductRepository.ExistsWithNormalizedNameInCategoryAsync(string n, Guid categoryId, Guid? e, CancellationToken ct) =>
            Task.FromResult(Products.Any(p => p.CategoryId == categoryId && p.NormalizedName == n && p.Id != e));
        Task IProductRepository.AddAsync(Product product, CancellationToken ct) { Products.Add(product); return Task.CompletedTask; }

        public Task SaveChangesAsync(CancellationToken ct = default) { Saves++; return Task.CompletedTask; }
    }
}
