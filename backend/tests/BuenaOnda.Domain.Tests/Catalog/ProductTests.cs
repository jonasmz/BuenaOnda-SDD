using BuenaOnda.Domain.Catalog;
using BuenaOnda.Domain.Common;

namespace BuenaOnda.Domain.Tests.Catalog;

public class ProductTests
{
    private static readonly Category Bebidas = Category.Create("Bebidas", null);

    private static Category Inactive(string name)
    {
        var category = Category.Create(name, null);
        typeof(Category).GetProperty(nameof(Category.IsActive))!.SetValue(category, false);
        return category;
    }

    private static Product NewProduct(string? image = null, bool available = true) =>
        Product.Create("Agua mineral", null, image, Bebidas, 1500m, available);

    [Fact]
    public void Product_without_characteristics_has_exactly_one_option_with_own_price()
    {
        var product = NewProduct();

        var option = Assert.Single(product.Options);
        Assert.Equal(1500m, option.Price);
        Assert.True(option.IsActive);
        Assert.True(product.IsActive);
        Assert.Equal(Bebidas.Id, product.CategoryId);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(" ")]
    public void Name_is_required(string? name) =>
        Assert.Throws<ValidationException>(() => Product.Create(name, null, null, Bebidas, 1m, true));

    [Theory]
    [InlineData(-0.01)]
    public void Negative_price_is_rejected(double price) =>
        Assert.Throws<ValidationException>(() => Product.Create("X", null, null, Bebidas, (decimal)price, true));

    [Fact]
    public void Zero_price_is_valid() => Assert.Equal(0m, Product.Create("X", null, null, Bebidas, 0m, true).Options[0].Price);

    [Fact]
    public void Price_and_availability_are_required()
    {
        Assert.Throws<ValidationException>(() => Product.Create("X", null, null, Bebidas, null, true));
        Assert.Throws<ValidationException>(() => Product.Create("X", null, null, Bebidas, 1m, null));
    }

    [Fact]
    public void Image_is_optional_and_must_be_http_url()
    {
        Assert.False(NewProduct().HasImage);
        Assert.True(NewProduct("https://cdn.example.com/agua.png").HasImage);
        Assert.Throws<ValidationException>(() => NewProduct("no-es-url"));
    }

    [Fact]
    public void Product_cannot_be_created_in_inactive_category() =>
        Assert.Throws<ConflictException>(() => Product.Create("X", null, null, Inactive("Vieja"), 1m, true));

    [Fact]
    public void Without_image_product_is_not_visible_to_public_but_can_be_available()
    {
        var product = NewProduct();

        Assert.False(product.IsVisibleToPublic(Bebidas));
        Assert.True(product.IsAvailable(Bebidas));
    }

    [Fact]
    public void With_image_product_is_visible_and_option_visibility_follows_product()
    {
        var product = NewProduct("https://cdn.example.com/agua.png");

        Assert.True(product.IsVisibleToPublic(Bebidas));
        Assert.True(product.Options[0].IsVisibleToPublic(product, Bebidas));
        Assert.False(product.IsVisibleToPublic(Inactive("Otra")));
    }

    [Fact]
    public void Option_marked_unavailable_makes_product_unavailable() =>
        Assert.False(NewProduct(available: false).IsAvailable(Bebidas));

    [Fact]
    public void Update_changes_commercial_information_and_moves_category()
    {
        var product = NewProduct();
        var postres = Category.Create("Postres", null);

        product.Update("Agua", "Sin gas", "https://cdn.example.com/a.png", Bebidas, postres);

        Assert.Equal("Agua", product.Name);
        Assert.Equal("Sin gas", product.Description);
        Assert.True(product.HasImage);
        Assert.Equal(postres.Id, product.CategoryId);
    }

    [Fact]
    public void Update_rejects_inactive_target_category_and_inactive_current_category()
    {
        var product = NewProduct();

        Assert.Throws<ConflictException>(() => product.Update("X", null, null, Bebidas, Inactive("Muerta")));
        Assert.Throws<ConflictException>(() => product.Update("X", null, null, Inactive("Muerta"), Bebidas));
    }
}
