using BuenaOnda.Domain.Catalog;
using BuenaOnda.Domain.Common;

namespace BuenaOnda.Domain.Tests.Catalog;

public class CatalogLifecycleTests
{
    private static readonly Category Active = Category.Create("Activa", null);

    private static OptionDraft Opt(decimal price, string? size = null) =>
        new(size is null ? null : new Dictionary<string, string> { ["tamaño"] = size }, price, true);

    private static Product Sized() =>
        Product.Create("Papas", null, "https://cdn.example.com/p.png", Active, ["tamaño"], [Opt(1000, "chica"), Opt(1800, "grande")]);

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
    public void Deactivating_product_keeps_manual_marks_but_makes_it_unavailable_and_invisible()
    {
        var product = Sized();

        product.Deactivate();

        Assert.All(product.Options, o => Assert.True(o.IsMarkedAvailable));
        Assert.False(product.IsAvailable(Active));
        Assert.False(product.IsVisibleToPublic(Active));
        product.Reactivate();
        Assert.True(product.IsAvailable(Active));
        Assert.True(product.IsVisibleToPublic(Active));
    }

    [Fact]
    public void Inactive_category_makes_product_unavailable_without_changing_marks()
    {
        var product = Sized();
        var category = Category.Create("Otra", null);
        category.Deactivate();

        Assert.False(product.IsAvailable(category));
        Assert.All(product.Options, o => Assert.True(o.IsMarkedAvailable));
    }

    [Fact]
    public void Inactive_product_or_category_rejects_structure_changes()
    {
        var product = Sized();
        var optionId = product.Options[0].Id;
        var inactiveCategory = Inactive();

        Assert.Throws<ConflictException>(() => product.SetOptionAvailability(optionId, false, inactiveCategory));
        Assert.Throws<ConflictException>(() => product.AddOption(Opt(1, "mediana"), inactiveCategory));
        product.Deactivate();
        Assert.Throws<ConflictException>(() => product.UpdateOption(optionId, Opt(5, "chica"), Active));
        Assert.Throws<ConflictException>(() => product.DeactivateOption(optionId, Active));
        Assert.Throws<ConflictException>(() => product.RemoveOption(optionId, false, Active));
        Assert.Throws<ConflictException>(() => product.Update("X", null, null, Active, Active));
    }

    [Fact]
    public void UpdateOption_changes_price_and_values_keeping_id_and_others_untouched()
    {
        var product = Sized();
        var first = product.Options[0];
        var second = product.Options[1];

        product.UpdateOption(first.Id, new OptionDraft(new Dictionary<string, string> { ["tamaño"] = "mini" }, 700m, true, "Chiquita", null), Active);

        Assert.Equal(700m, first.Price);
        Assert.Equal("mini", first.Values[0].Value);
        Assert.Equal("Chiquita", first.Description);
        Assert.Equal(1800m, second.Price);
    }

    [Fact]
    public void UpdateOption_rejects_values_of_another_option_even_if_inactive_and_leaves_state_intact()
    {
        var product = Sized();
        product.DeactivateOption(product.Options[1].Id, Active);

        Assert.Throws<ConflictException>(() => product.UpdateOption(product.Options[0].Id, Opt(1, " GRANDE "), Active));
        Assert.Equal("chica", product.Options[0].Values[0].Value);
        Assert.Throws<ValidationException>(() => product.UpdateOption(product.Options[0].Id, Opt(-1, "chica"), Active));
        Assert.Equal(1000m, product.Options[0].Price);
    }

    [Fact]
    public void Option_availability_is_manual_and_does_not_change_with_deactivation()
    {
        var product = Sized();
        var option = product.Options[0];

        product.SetOptionAvailability(option.Id, false, Active);
        product.DeactivateOption(option.Id, Active);
        product.ReactivateOption(option.Id, Active);

        Assert.False(option.IsMarkedAvailable);
        Assert.True(product.IsAvailable(Active)); // la otra opción sigue disponible
        product.SetOptionAvailability(product.Options[1].Id, false, Active);
        Assert.False(product.IsAvailable(Active));
        Assert.Throws<ValidationException>(() => product.SetOptionAvailability(option.Id, null, Active));
    }

    [Fact]
    public void Inactive_option_is_not_available_or_visible_and_still_reserves_values()
    {
        var product = Sized();
        var option = product.Options[0];

        product.DeactivateOption(option.Id, Active);

        Assert.False(option.IsAvailable(product, Active));
        Assert.False(option.IsVisibleToPublic(product, Active));
        Assert.Throws<ConflictException>(() => product.AddOption(Opt(1, "chica"), Active));
        product.ReactivateOption(option.Id, Active);
        Assert.True(option.IsAvailable(product, Active));
    }

    [Fact]
    public void RemoveOption_only_if_not_referenced_and_not_last()
    {
        var product = Sized();
        var first = product.Options[0].Id;

        Assert.Throws<ConflictException>(() => product.RemoveOption(first, true, Active));
        Assert.Equal(2, product.Options.Count);
        product.RemoveOption(first, false, Active);
        Assert.Single(product.Options);
        Assert.Throws<ConflictException>(() => product.RemoveOption(product.Options[0].Id, false, Active));
        Assert.Throws<NotFoundException>(() => product.RemoveOption(Guid.NewGuid(), false, Active));
    }

    [Fact]
    public void Inactive_options_count_as_options_for_the_last_option_rule()
    {
        var product = Sized();
        product.DeactivateOption(product.Options[0].Id, Active);

        product.RemoveOption(product.Options[1].Id, false, Active);

        Assert.Single(product.Options);
    }
}
