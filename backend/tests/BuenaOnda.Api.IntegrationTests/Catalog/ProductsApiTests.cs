using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BuenaOnda.Api.IntegrationTests.Catalog;

public class ProductsApiTests(CatalogApiFixture fixture) : IClassFixture<CatalogApiFixture>
{
    private const string Categories = "/api/admin/catalog/categories";
    private const string Products = "/api/admin/catalog/products";

    private record IdDto(Guid Id);
    private record OptionDto(Guid Id, decimal Price, bool IsMarkedAvailable, bool IsActive, bool IsAvailable, bool IsVisibleToPublic);
    private record ProductDto(
        Guid Id, string Name, string? ImageUrl, bool HasImage, Guid CategoryId, bool IsActive,
        bool IsAvailable, bool IsVisibleToPublic, List<OptionDto> Options);

    private static string Unique(string prefix) => $"{prefix}-{Guid.NewGuid():N}";

    private async Task<Guid> CreateCategoryAsync()
    {
        var response = await fixture.CreateClient().PostAsJsonAsync(Categories, new { name = Unique("Cat") });
        return (await response.Content.ReadFromJsonAsync<IdDto>())!.Id;
    }

    private Task<HttpResponseMessage> PostProductAsync(Guid categoryId, string name, string? imageUrl = null, decimal price = 1200m) =>
        fixture.CreateClient().PostAsJsonAsync(Products, new
        {
            name, imageUrl, categoryId,
            options = new[] { new { price, isMarkedAvailable = true } },
        });

    [Fact]
    public async Task Create_without_image_is_available_but_not_visible_and_has_one_option()
    {
        var response = await PostProductAsync(await CreateCategoryAsync(), Unique("Agua"));
        var product = (await response.Content.ReadFromJsonAsync<ProductDto>())!;

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.False(product.HasImage);
        Assert.False(product.IsVisibleToPublic);
        Assert.True(product.IsAvailable);
        Assert.Equal(1200m, Assert.Single(product.Options).Price);
    }

    [Fact]
    public async Task Assigning_image_makes_product_visible()
    {
        var categoryId = await CreateCategoryAsync();
        var created = (await (await PostProductAsync(categoryId, Unique("Agua"))).Content.ReadFromJsonAsync<ProductDto>())!;

        var response = await fixture.CreateClient().PutAsJsonAsync($"{Products}/{created.Id}",
            new { name = created.Name, imageUrl = "https://cdn.example.com/agua.png", categoryId });
        var updated = (await response.Content.ReadFromJsonAsync<ProductDto>())!;
        var fetched = await fixture.CreateClient().GetFromJsonAsync<ProductDto>($"{Products}/{created.Id}");

        Assert.True(updated.IsVisibleToPublic);
        Assert.Equal(updated.Options[0].Id, fetched!.Options[0].Id);
        Assert.True(fetched.Options[0].IsVisibleToPublic);
    }

    [Fact]
    public async Task Repeated_name_in_same_category_is_conflict_but_ok_in_another()
    {
        var first = await CreateCategoryAsync();
        var second = await CreateCategoryAsync();
        var name = Unique("Gaseosa");
        await PostProductAsync(first, name);

        Assert.Equal(HttpStatusCode.Conflict, (await PostProductAsync(first, $" {name.ToUpperInvariant()} ")).StatusCode);
        Assert.Equal(HttpStatusCode.Created, (await PostProductAsync(second, name)).StatusCode);
    }

    [Fact]
    public async Task Inactive_category_rejects_new_products()
    {
        var categoryId = await CreateCategoryAsync();
        await using var scope = fixture.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<BuenaOnda.Infrastructure.Persistence.CatalogDbContext>();
        await db.Database.ExecuteSqlInterpolatedAsync($"UPDATE categories SET \"IsActive\" = false WHERE \"Id\" = {categoryId}");

        Assert.Equal(HttpStatusCode.Conflict, (await PostProductAsync(categoryId, Unique("X"))).StatusCode);
    }

    [Fact]
    public async Task Moving_to_category_where_name_exists_is_conflict()
    {
        var first = await CreateCategoryAsync();
        var second = await CreateCategoryAsync();
        var name = Unique("Cerveza");
        var moving = (await (await PostProductAsync(first, name)).Content.ReadFromJsonAsync<ProductDto>())!;
        await PostProductAsync(second, name);

        var response = await fixture.CreateClient().PutAsJsonAsync($"{Products}/{moving.Id}", new { name, categoryId = second });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Invalid_data_is_bad_request_and_unknown_product_is_not_found()
    {
        var categoryId = await CreateCategoryAsync();
        var client = fixture.CreateClient();

        Assert.Equal(HttpStatusCode.BadRequest, (await PostProductAsync(categoryId, " ")).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await PostProductAsync(categoryId, Unique("N"), price: -1m)).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await PostProductAsync(categoryId, Unique("N"), imageUrl: "nope")).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsJsonAsync(Products, new { name = "X", categoryId })).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync($"{Products}/{Guid.NewGuid()}")).StatusCode);
    }

    [Fact]
    public async Task List_filters_by_category()
    {
        var categoryId = await CreateCategoryAsync();
        await PostProductAsync(categoryId, Unique("A"));
        await PostProductAsync(categoryId, Unique("B"));

        var list = await fixture.CreateClient().GetFromJsonAsync<List<ProductDto>>($"{Products}?categoryId={categoryId}");

        Assert.Equal(2, list!.Count);
    }
}
