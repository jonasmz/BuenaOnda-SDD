using BuenaOnda.Domain.Catalog;
using BuenaOnda.Domain.Common;

namespace BuenaOnda.Domain.Tests.Catalog;

public class ProductTests
{
    private static readonly Category Bebidas = Category.Create("Bebidas", null);

    private static Category Inactive(string name)
    {
        var category = Category.Create(name, null);
        category.Deactivate();
        return category;
    }

    private static Product NewProduct(string? image = null, bool available = true) =>
        Product.Create("Agua mineral 500 ml", null, image, Bebidas, 1500m, available);

    [Fact]
    public void Create_starts_active_with_own_price_and_trimmed_normalized_name()
    {
        var product = Product.Create("  Papas Fritas Grandes ", "  Con sal ", null, Bebidas, 1800m, true);

        Assert.NotEqual(Guid.Empty, product.Id);
        Assert.True(product.IsActive);
        Assert.Equal("Papas Fritas Grandes", product.Name);
        Assert.Equal("papas fritas grandes", product.NormalizedName);
        Assert.Equal("Con sal", product.Description);
        Assert.Equal(1800m, product.Price);
        Assert.Equal(Bebidas.Id, product.CategoryId);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(" ")]
    public void Name_is_required(string? name) =>
        Assert.Throws<ValidationException>(() => Product.Create(name, null, null, Bebidas, 1m, true));

    [Fact]
    public void Negative_price_is_rejected_and_zero_is_valid()
    {
        Assert.Throws<ValidationException>(() => Product.Create("X", null, null, Bebidas, -0.01m, true));
        Assert.Equal(0m, Product.Create("X", null, null, Bebidas, 0m, true).Price);
    }

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
    public void With_image_product_is_visible_only_while_its_category_is_active()
    {
        var product = NewProduct("https://cdn.example.com/agua.png");

        Assert.True(product.IsVisibleToPublic(Bebidas));
        Assert.False(product.IsVisibleToPublic(Inactive("Otra")));
    }

    [Fact]
    public void Product_marked_unavailable_is_unavailable() =>
        Assert.False(NewProduct(available: false).IsAvailable(Bebidas));

    [Fact]
    public void Update_changes_information_price_and_category_keeping_id()
    {
        var product = NewProduct();
        var id = product.Id;
        var postres = Category.Create("Postres", null);

        product.Update("Agua", "Sin gas", "https://cdn.example.com/a.png", 1700m, Bebidas, postres);

        Assert.Equal(id, product.Id);
        Assert.Equal("Agua", product.Name);
        Assert.Equal("Sin gas", product.Description);
        Assert.True(product.HasImage);
        Assert.Equal(1700m, product.Price);
        Assert.Equal(postres.Id, product.CategoryId);
    }

    [Fact]
    public void Failed_update_leaves_product_untouched()
    {
        var product = NewProduct();

        Assert.Throws<ValidationException>(() => product.Update("Otro", null, "nope", 5m, Bebidas, Bebidas));
        Assert.Throws<ValidationException>(() => product.Update("Otro", null, null, -1m, Bebidas, Bebidas));

        Assert.Equal("Agua mineral 500 ml", product.Name);
        Assert.Equal(1500m, product.Price);
    }

    [Fact]
    public void Update_rejects_inactive_target_category_and_inactive_current_category()
    {
        var product = NewProduct();

        Assert.Throws<ConflictException>(() => product.Update("X", null, null, 1m, Bebidas, Inactive("Muerta")));
        Assert.Throws<ConflictException>(() => product.Update("X", null, null, 1m, Inactive("Muerta"), Bebidas));
    }
}
