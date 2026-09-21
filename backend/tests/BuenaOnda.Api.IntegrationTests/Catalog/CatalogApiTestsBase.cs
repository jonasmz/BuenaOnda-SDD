using System.Net;
using System.Net.Http.Json;

namespace BuenaOnda.Api.IntegrationTests.Catalog;

public abstract class CatalogApiTestsBase(CatalogApiFixture fixture)
{
    protected const string Categories = "/api/admin/catalog/categories";
    protected const string Products = "/api/admin/catalog/products";

    protected record IdDto(Guid Id);
    protected record CategoryDto(Guid Id, string Name, bool IsActive);
    protected record ProductDto(
        Guid Id, string Name, string? ImageUrl, bool HasImage, Guid CategoryId, decimal Price, bool IsActive,
        bool IsMarkedAvailable, bool IsAvailable, bool IsVisibleToPublic);

    protected HttpClient Client { get; } = fixture.CreateClient();

    protected static string Unique(string prefix) => $"{prefix}-{Guid.NewGuid():N}";

    protected async Task<Guid> CategoryAsync(string? name = null)
    {
        var response = await Client.PostAsJsonAsync(Categories, new { name = name ?? Unique("Cat") });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<IdDto>())!.Id;
    }

    protected Task<HttpResponseMessage> PostProductAsync(
        Guid categoryId, string name, string? imageUrl = null, decimal? price = 1200m, bool? available = true) =>
        Client.PostAsJsonAsync(Products, new { name, imageUrl, categoryId, price, isMarkedAvailable = available });

    protected async Task<ProductDto> ProductAsync(Guid? categoryId = null, string? name = null, string? imageUrl = "https://cdn.example.com/p.png")
    {
        var response = await PostProductAsync(categoryId ?? await CategoryAsync(), name ?? Unique("P"), imageUrl);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<ProductDto>())!;
    }

    protected async Task<ProductDto> GetAsync(Guid id) => (await Client.GetFromJsonAsync<ProductDto>($"{Products}/{id}"))!;
}
