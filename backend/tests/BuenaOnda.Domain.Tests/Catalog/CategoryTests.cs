using BuenaOnda.Domain.Catalog;
using BuenaOnda.Domain.Common;

namespace BuenaOnda.Domain.Tests.Catalog;

public class CategoryTests
{
    [Fact]
    public void Create_starts_active_with_trimmed_name_and_normalized_name()
    {
        var category = Category.Create("  Bebidas ", "  Frías ");

        Assert.NotEqual(Guid.Empty, category.Id);
        Assert.True(category.IsActive);
        Assert.Equal("Bebidas", category.Name);
        Assert.Equal("bebidas", category.NormalizedName);
        Assert.Equal("Frías", category.Description);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_rejects_missing_name(string? name) =>
        Assert.Throws<ValidationException>(() => Category.Create(name, null));

    [Fact]
    public void Blank_description_is_stored_as_null() =>
        Assert.Null(Category.Create("Postres", "  ").Description);

    [Fact]
    public void Update_changes_name_and_description_keeping_id()
    {
        var category = Category.Create("Bebidas", null);
        var id = category.Id;

        category.Update("Tragos", "Con alcohol");

        Assert.Equal(id, category.Id);
        Assert.Equal("Tragos", category.Name);
        Assert.Equal("tragos", category.NormalizedName);
        Assert.Equal("Con alcohol", category.Description);
    }

    [Fact]
    public void Update_rejects_empty_name_without_changing_state()
    {
        var category = Category.Create("Bebidas", null);

        Assert.Throws<ValidationException>(() => category.Update(" ", null));
        Assert.Equal("Bebidas", category.Name);
    }

    [Fact]
    public void Accents_remain_significant_in_normalized_name() =>
        Assert.NotEqual(Category.Create("Café", null).NormalizedName, Category.Create("Cafe", null).NormalizedName);
}
