using BuenaOnda.Domain.Catalog;
using BuenaOnda.Domain.Common;

namespace BuenaOnda.Domain.Tests.Catalog;

public class CatalogLifecycleTests
{
    private static readonly Category Active = Category.Create("Activa", null);

    private static Product New(bool available = true) =>
        Product.Create("Papas fritas grandes", null, "https://cdn.example.com/p.png", Active, 1800m, available);

    private static Category Inactive()
    {
        var category = Category.Create("Inactiva", null);
        category.Deactivate();
        return category;
    }

    [Fact]
    public void Category_deactivates_and_reactivates()
    {
        var category = Category.Create("X", null);
        category.Deactivate();
        Assert.False(category.IsActive);
        category.Reactivate();
        Assert.True(category.IsActive);
    }

    [Fact]
    public void Deactivating_product_keeps_manual_mark_but_makes_it_unavailable_and_invisible()
    {
        var product = New();

        product.Deactivate();

        Assert.True(product.IsMarkedAvailable);
        Assert.False(product.IsAvailable(Active));
        Assert.False(product.IsVisibleToPublic(Active));
        product.Reactivate();
        Assert.True(product.IsAvailable(Active));
        Assert.True(product.IsVisibleToPublic(Active));
    }

    [Fact]
    public void Reactivation_restores_the_previous_manual_mark()
    {
        var product = New(available: false);

        product.Deactivate();
        product.Reactivate();

        Assert.False(product.IsMarkedAvailable);
        Assert.False(product.IsAvailable(Active));
    }

    [Fact]
    public void Inactive_category_makes_product_unavailable_without_changing_the_mark()
    {
        var product = New();

        Assert.False(product.IsAvailable(Inactive()));
        Assert.True(product.IsMarkedAvailable);
    }

    [Fact]
    public void Availability_is_manual_and_required()
    {
        var product = New();

        product.SetAvailability(false, Active);
        Assert.False(product.IsAvailable(Active));
        product.SetAvailability(true, Active);
        Assert.True(product.IsAvailable(Active));
        Assert.Throws<ValidationException>(() => product.SetAvailability(null, Active));
    }

    [Fact]
    public void Inactive_product_or_category_rejects_modifications_but_allows_reactivation()
    {
        var product = New();

        Assert.Throws<ConflictException>(() => product.SetAvailability(false, Inactive()));
        Assert.Throws<ConflictException>(() => product.Update("X", null, null, 1m, Inactive(), Active));
        product.Deactivate();
        Assert.Throws<ConflictException>(() => product.SetAvailability(false, Active));
        Assert.Throws<ConflictException>(() => product.Update("X", null, null, 1m, Active, Active));
        product.Reactivate();
        product.SetAvailability(false, Active);
    }
}
