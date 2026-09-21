using System.Net;
using System.Net.Http.Json;

namespace BuenaOnda.Api.IntegrationTests.Catalog;

public class CategoriesApiTests(CatalogApiFixture fixture) : IClassFixture<CatalogApiFixture>
{
    private const string Route = "/api/admin/catalog/categories";

    private record CategoryDto(Guid Id, string Name, string? Description, bool IsActive);

    private async Task<CategoryDto> CreateAsync(string name, string? description = null)
    {
        var response = await fixture.CreateClient().PostAsJsonAsync(Route, new { name, description });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<CategoryDto>())!;
    }

    private static string Unique(string prefix) => $"{prefix}-{Guid.NewGuid():N}";

    [Fact]
    public async Task Create_then_get_and_list_without_products()
    {
        var name = Unique("Bebidas");
        var created = await CreateAsync(name, "Frías");
        var client = fixture.CreateClient();

        var fetched = await client.GetFromJsonAsync<CategoryDto>($"{Route}/{created.Id}");
        var list = await client.GetFromJsonAsync<List<CategoryDto>>(Route);

        Assert.Equal(created, fetched);
        Assert.True(created.IsActive);
        Assert.Contains(list!, c => c.Id == created.Id);
    }

    [Fact]
    public async Task Update_changes_name_and_description_keeping_id()
    {
        var created = await CreateAsync(Unique("Postres"));
        var newName = Unique("Dulces");

        var response = await fixture.CreateClient().PutAsJsonAsync($"{Route}/{created.Id}", new { name = newName, description = "Con azúcar" });
        var updated = await response.Content.ReadFromJsonAsync<CategoryDto>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(created.Id, updated!.Id);
        Assert.Equal(newName, updated.Name);
        Assert.Equal("Con azúcar", updated.Description);
    }

    [Fact]
    public async Task Repeated_name_with_different_capitalization_is_conflict()
    {
        var name = Unique("Ensaladas");
        await CreateAsync(name);

        var response = await fixture.CreateClient().PostAsJsonAsync(Route, new { name = $"  {name.ToUpperInvariant()} " });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Update_to_name_of_another_category_is_conflict()
    {
        var first = await CreateAsync(Unique("Uno"));
        var second = await CreateAsync(Unique("Dos"));

        var response = await fixture.CreateClient().PutAsJsonAsync($"{Route}/{second.Id}", new { name = first.Name.ToUpperInvariant() });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Empty_name_is_bad_request()
    {
        var response = await fixture.CreateClient().PostAsJsonAsync(Route, new { name = "  " });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Unknown_category_is_not_found()
    {
        var client = fixture.CreateClient();

        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync($"{Route}/{Guid.NewGuid()}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound,
            (await client.PutAsJsonAsync($"{Route}/{Guid.NewGuid()}", new { name = "X" })).StatusCode);
    }
}
