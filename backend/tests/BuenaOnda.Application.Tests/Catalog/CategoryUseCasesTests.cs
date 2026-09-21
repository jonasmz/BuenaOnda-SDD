using BuenaOnda.Application.Catalog.Categories;
using BuenaOnda.Application.Catalog.Ports;
using BuenaOnda.Domain.Catalog;
using BuenaOnda.Domain.Common;

namespace BuenaOnda.Application.Tests.Catalog;

public class CategoryUseCasesTests
{
    private readonly FakeCategoryRepository _repository = new();
    private readonly FakeUnitOfWork _unitOfWork = new();

    [Fact]
    public async Task Create_persists_and_saves()
    {
        var view = await new CreateCategory(_repository, _unitOfWork).ExecuteAsync("Bebidas", null);

        Assert.True(view.IsActive);
        Assert.Single(_repository.Items);
        Assert.Equal(1, _unitOfWork.Saves);
    }

    [Fact]
    public async Task Create_rejects_name_repeated_with_different_case_and_spaces()
    {
        var create = new CreateCategory(_repository, _unitOfWork);
        await create.ExecuteAsync("Bebidas", null);

        await Assert.ThrowsAsync<ConflictException>(() => create.ExecuteAsync("  BEBIDAS ", null));
        Assert.Single(_repository.Items);
    }

    [Fact]
    public async Task Update_allows_keeping_own_name_with_other_capitalization()
    {
        var created = await new CreateCategory(_repository, _unitOfWork).ExecuteAsync("Bebidas", null);

        var updated = await new UpdateCategory(_repository, _unitOfWork).ExecuteAsync(created.Id, "BEBIDAS", "Frías");

        Assert.Equal("BEBIDAS", updated.Name);
        Assert.Equal("Frías", updated.Description);
    }

    [Fact]
    public async Task Update_rejects_name_of_another_category()
    {
        var create = new CreateCategory(_repository, _unitOfWork);
        await create.ExecuteAsync("Bebidas", null);
        var other = await create.ExecuteAsync("Postres", null);

        await Assert.ThrowsAsync<ConflictException>(() =>
            new UpdateCategory(_repository, _unitOfWork).ExecuteAsync(other.Id, "bebidas", null));
    }

    [Fact]
    public async Task Get_and_update_unknown_category_throw_not_found()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => new GetCategory(_repository).ExecuteAsync(Guid.NewGuid()));
        await Assert.ThrowsAsync<NotFoundException>(() =>
            new UpdateCategory(_repository, _unitOfWork).ExecuteAsync(Guid.NewGuid(), "X", null));
    }

    [Fact]
    public async Task List_hides_inactive_unless_requested()
    {
        _repository.Items.Add(Category.Create("Activa", null));
        var inactive = Category.Create("Inactiva", null);
        typeof(Category).GetProperty(nameof(Category.IsActive))!.SetValue(inactive, false);
        _repository.Items.Add(inactive);

        var list = new ListCategories(_repository);

        Assert.Single(await list.ExecuteAsync(false));
        Assert.Equal(2, (await list.ExecuteAsync(true)).Count);
    }

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public int Saves { get; private set; }

        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            Saves++;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeCategoryRepository : ICategoryRepository
    {
        public List<Category> Items { get; } = [];

        public Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult(Items.FirstOrDefault(c => c.Id == id));

        public Task<IReadOnlyList<Category>> ListAsync(bool includeInactive, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<Category>>(Items.Where(c => includeInactive || c.IsActive).ToList());

        public Task<bool> ExistsWithNormalizedNameAsync(string normalizedName, Guid? excludingId, CancellationToken cancellationToken = default) =>
            Task.FromResult(Items.Any(c => c.NormalizedName == normalizedName && c.Id != excludingId));

        public Task AddAsync(Category category, CancellationToken cancellationToken = default)
        {
            Items.Add(category);
            return Task.CompletedTask;
        }
    }
}
