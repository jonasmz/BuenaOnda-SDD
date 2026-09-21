using System.Net;
using System.Net.Http.Json;

namespace BuenaOnda.Api.IntegrationTests.Catalog;

public class ProductsApiTests(CatalogApiFixture fixture) : CatalogApiTestsBase(fixture), IClassFixture<CatalogApiFixture>
{
    [Fact]
    public async Task Create_without_image_is_available_but_not_visible_and_has_its_own_price()
    {
        var response = await PostProductAsync(await CategoryAsync(), Unique("Papas fritas chicas"), price: 1000m);
        var product = (await response.Content.ReadFromJsonAsync<ProductDto>())!;

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.False(product.HasImage);
        Assert.False(product.IsVisibleToPublic);
        Assert.True(product.IsAvailable);
        Assert.Equal(1000m, product.Price);
    }

    [Fact]
    public async Task Assigning_image_makes_product_visible()
    {
        var categoryId = await CategoryAsync();
        var created = await ProductAsync(categoryId, imageUrl: null);

        var response = await Client.PutAsJsonAsync($"{Products}/{created.Id}",
            new { name = created.Name, imageUrl = "https://cdn.example.com/agua.png", categoryId, price = created.Price });
        var updated = (await response.Content.ReadFromJsonAsync<ProductDto>())!;

        Assert.False(created.IsVisibleToPublic);
        Assert.True(updated.IsVisibleToPublic);
        Assert.True((await GetAsync(created.Id)).IsVisibleToPublic);
    }

    [Fact]
    public async Task Modifying_price_returns_the_current_price()
    {
        var categoryId = await CategoryAsync();
        var created = await ProductAsync(categoryId);

        await Client.PutAsJsonAsync($"{Products}/{created.Id}", new { name = created.Name, categoryId, price = 1999.5m });

        Assert.Equal(1999.5m, (await GetAsync(created.Id)).Price);
    }

    [Fact]
    public async Task Repeated_name_in_same_category_is_conflict_but_ok_in_another()
    {
        var first = await CategoryAsync();
        var second = await CategoryAsync();
        var name = Unique("Gaseosa");
        await PostProductAsync(first, name);

        Assert.Equal(HttpStatusCode.Conflict, (await PostProductAsync(first, $" {name.ToUpperInvariant()} ")).StatusCode);
        Assert.Equal(HttpStatusCode.Created, (await PostProductAsync(second, name)).StatusCode);
    }

    [Fact]
    public async Task Inactive_category_rejects_new_products()
    {
        var categoryId = await CategoryAsync();
        await Client.PostAsync($"{Categories}/{categoryId}/deactivate", null);

        Assert.Equal(HttpStatusCode.Conflict, (await PostProductAsync(categoryId, Unique("X"))).StatusCode);
    }

    [Fact]
    public async Task Moving_to_category_where_name_exists_is_conflict()
    {
        var first = await CategoryAsync();
        var second = await CategoryAsync();
        var name = Unique("Cerveza");
        var moving = await ProductAsync(first, name);
        await PostProductAsync(second, name);

        var response = await Client.PutAsJsonAsync($"{Products}/{moving.Id}", new { name, categoryId = second, price = 1m });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Invalid_data_is_bad_request_and_unknown_product_is_not_found()
    {
        var categoryId = await CategoryAsync();

        Assert.Equal(HttpStatusCode.BadRequest, (await PostProductAsync(categoryId, " ")).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await PostProductAsync(categoryId, Unique("N"), price: -1m)).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await PostProductAsync(categoryId, Unique("N"), price: null)).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await PostProductAsync(categoryId, Unique("N"), available: null)).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await PostProductAsync(categoryId, Unique("N"), imageUrl: "nope")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await Client.GetAsync($"{Products}/{Guid.NewGuid()}")).StatusCode);
    }

    [Fact]
    public async Task List_filters_by_category()
    {
        var categoryId = await CategoryAsync();
        await PostProductAsync(categoryId, Unique("A"));
        await PostProductAsync(categoryId, Unique("B"));

        var list = await Client.GetFromJsonAsync<List<ProductDto>>($"{Products}?categoryId={categoryId}");

        Assert.Equal(2, list!.Count);
    }
}
