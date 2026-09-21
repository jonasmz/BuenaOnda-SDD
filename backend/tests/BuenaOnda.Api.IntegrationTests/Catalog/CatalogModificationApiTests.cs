using System.Net;
using System.Net.Http.Json;
using BuenaOnda.Application.Catalog.Ports;
using Microsoft.Extensions.DependencyInjection;

namespace BuenaOnda.Api.IntegrationTests.Catalog;

/// <summary>Fixture que simula una funcionalidad consumidora que ya referenció todas las opciones.</summary>
public sealed class ReferencedOptionsFixture : CatalogApiFixture
{
    protected override void ConfigureTestServices(IServiceCollection services) =>
        services.AddScoped<IOptionReferenceChecker, AlwaysReferenced>();

    private sealed class AlwaysReferenced : IOptionReferenceChecker
    {
        public Task<bool> IsReferencedAsync(Guid optionId, CancellationToken cancellationToken = default) => Task.FromResult(true);
    }
}

public abstract class CatalogApiTestsBase(CatalogApiFixture fixture)
{
    protected const string Categories = "/api/admin/catalog/categories";
    protected const string Products = "/api/admin/catalog/products";

    protected record IdDto(Guid Id);
    protected record CategoryDto(Guid Id, bool IsActive);
    protected record OptionDto(
        Guid Id, Dictionary<string, string> Values, decimal Price, bool IsMarkedAvailable, bool IsActive, bool IsAvailable, bool IsVisibleToPublic);
    protected record ProductDto(
        Guid Id, Guid CategoryId, bool IsActive, bool IsAvailable, bool IsVisibleToPublic, List<OptionDto> Options);

    protected HttpClient Client { get; } = fixture.CreateClient();

    protected static string Unique(string prefix) => $"{prefix}-{Guid.NewGuid():N}";

    protected async Task<Guid> CategoryAsync() =>
        (await (await Client.PostAsJsonAsync(Categories, new { name = Unique("C") })).Content.ReadFromJsonAsync<IdDto>())!.Id;

    protected async Task<ProductDto> ProductAsync(Guid? categoryId = null, params (string Size, decimal Price)[] sizes)
    {
        sizes = sizes.Length == 0 ? [("chica", 1000m), ("grande", 1800m)] : sizes;
        var response = await Client.PostAsJsonAsync(Products, new
        {
            name = Unique("P"),
            categoryId = categoryId ?? await CategoryAsync(),
            imageUrl = "https://cdn.example.com/p.png",
            characteristics = new[] { "tamaño" },
            options = sizes.Select(s => new { values = new Dictionary<string, string> { ["tamaño"] = s.Size }, price = s.Price, isMarkedAvailable = true }),
        });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<ProductDto>())!;
    }

    protected async Task<ProductDto> GetAsync(Guid id) => (await Client.GetFromJsonAsync<ProductDto>($"{Products}/{id}"))!;
}

public class CatalogModificationApiTests(CatalogApiFixture fixture) : CatalogApiTestsBase(fixture), IClassFixture<CatalogApiFixture>
{
    [Fact]
    public async Task Modify_price_of_only_one_option_keeping_ids()
    {
        var product = await ProductAsync();
        var first = product.Options[0];

        var response = await Client.PutAsJsonAsync($"{Products}/{product.Id}/options/{first.Id}",
            new { values = first.Values, price = 1250m });
        var updated = (await response.Content.ReadFromJsonAsync<ProductDto>())!;

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(1250m, updated.Options.Single(o => o.Id == first.Id).Price);
        Assert.Equal(1800m, updated.Options.Single(o => o.Id != first.Id).Price);
    }

    [Fact]
    public async Task Modify_option_values_to_existing_ones_is_conflict_and_changing_them_works()
    {
        var product = await ProductAsync();
        var first = product.Options[0];
        var url = $"{Products}/{product.Id}/options/{first.Id}";

        var conflict = await Client.PutAsJsonAsync(url, new { values = new Dictionary<string, string> { ["tamaño"] = "GRANDE" }, price = 1m });
        var ok = await Client.PutAsJsonAsync(url, new { values = new Dictionary<string, string> { ["tamaño"] = "mediana" }, price = 1m });

        Assert.Equal(HttpStatusCode.Conflict, conflict.StatusCode);
        Assert.Equal(HttpStatusCode.OK, ok.StatusCode);
        Assert.Contains((await GetAsync(product.Id)).Options, o => o.Values["tamaño"] == "mediana");
    }

    [Fact]
    public async Task Availability_per_option_and_product_unavailable_when_none_available()
    {
        var product = await ProductAsync();
        foreach (var option in product.Options)
        {
            var response = await Client.PutAsJsonAsync($"{Products}/{product.Id}/options/{option.Id}/availability", new { isMarkedAvailable = false });
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        var fetched = await GetAsync(product.Id);

        Assert.False(fetched.IsAvailable);
        Assert.All(fetched.Options, o => Assert.False(o.IsAvailable));
        Assert.Equal(HttpStatusCode.BadRequest,
            (await Client.PutAsJsonAsync($"{Products}/{product.Id}/options/{product.Options[0].Id}/availability", new { })).StatusCode);
    }

    [Fact]
    public async Task Modifying_a_category_keeps_its_products()
    {
        var categoryId = await CategoryAsync();
        var product = await ProductAsync(categoryId);

        await Client.PutAsJsonAsync($"{Categories}/{categoryId}", new { name = Unique("Renombrada") });

        Assert.Equal(categoryId, (await GetAsync(product.Id)).CategoryId);
    }

    [Fact]
    public async Task Category_deactivation_hides_products_and_reactivation_restores_marks()
    {
        var categoryId = await CategoryAsync();
        var product = await ProductAsync(categoryId);
        await Client.PutAsJsonAsync($"{Products}/{product.Id}/options/{product.Options[0].Id}/availability", new { isMarkedAvailable = false });

        var deactivated = await Client.PostAsync($"{Categories}/{categoryId}/deactivate", null);
        var whileInactive = await GetAsync(product.Id);
        await Client.PostAsync($"{Categories}/{categoryId}/reactivate", null);
        var restored = await GetAsync(product.Id);

        Assert.False((await deactivated.Content.ReadFromJsonAsync<CategoryDto>())!.IsActive);
        Assert.False(whileInactive.IsAvailable);
        Assert.False(whileInactive.IsVisibleToPublic);
        Assert.True(restored.IsAvailable);
        Assert.True(restored.IsVisibleToPublic);
        Assert.False(restored.Options.Single(o => o.Id == product.Options[0].Id).IsMarkedAvailable);
        Assert.True(restored.Options.Single(o => o.Id == product.Options[1].Id).IsMarkedAvailable);
    }

    [Fact]
    public async Task Product_deactivation_is_reversible_and_blocks_modifications()
    {
        var product = await ProductAsync();
        var option = product.Options[0];

        var deactivated = (await (await Client.PostAsync($"{Products}/{product.Id}/deactivate", null)).Content.ReadFromJsonAsync<ProductDto>())!;
        var blocked = await Client.PutAsJsonAsync($"{Products}/{product.Id}/options/{option.Id}/availability", new { isMarkedAvailable = false });
        var reactivated = (await (await Client.PostAsync($"{Products}/{product.Id}/reactivate", null)).Content.ReadFromJsonAsync<ProductDto>())!;

        Assert.False(deactivated.IsActive);
        Assert.False(deactivated.IsAvailable);
        Assert.Equal(HttpStatusCode.Conflict, blocked.StatusCode);
        Assert.True(reactivated.IsActive);
        Assert.True(reactivated.IsAvailable);
    }

    [Fact]
    public async Task There_are_no_endpoints_to_delete_products_or_categories()
    {
        var product = await ProductAsync();

        Assert.Equal(HttpStatusCode.MethodNotAllowed, (await Client.DeleteAsync($"{Products}/{product.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.MethodNotAllowed, (await Client.DeleteAsync($"{Categories}/{product.CategoryId}")).StatusCode);
    }

    [Fact]
    public async Task Unreferenced_option_is_deleted_but_never_the_last_one()
    {
        var product = await ProductAsync();

        var deleted = await Client.DeleteAsync($"{Products}/{product.Id}/options/{product.Options[0].Id}");
        var last = await Client.DeleteAsync($"{Products}/{product.Id}/options/{product.Options[1].Id}");

        Assert.Equal(HttpStatusCode.NoContent, deleted.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, last.StatusCode);
        Assert.Single((await GetAsync(product.Id)).Options);
    }

    [Fact]
    public async Task Inactive_option_keeps_its_values_reserved_and_can_be_reactivated()
    {
        var product = await ProductAsync();
        var first = product.Options[0];

        await Client.PostAsync($"{Products}/{product.Id}/options/{first.Id}/deactivate", null);
        var recreate = await Client.PostAsJsonAsync($"{Products}/{product.Id}/options",
            new { values = first.Values, price = 1m, isMarkedAvailable = true });
        var whileInactive = (await GetAsync(product.Id)).Options.Single(o => o.Id == first.Id);
        await Client.PostAsync($"{Products}/{product.Id}/options/{first.Id}/reactivate", null);
        var reactivated = (await GetAsync(product.Id)).Options.Single(o => o.Id == first.Id);

        Assert.Equal(HttpStatusCode.Conflict, recreate.StatusCode);
        Assert.False(whileInactive.IsActive);
        Assert.False(whileInactive.IsAvailable);
        Assert.True(reactivated.IsAvailable);
    }

    [Fact]
    public async Task Unknown_option_is_not_found()
    {
        var product = await ProductAsync();

        Assert.Equal(HttpStatusCode.NotFound, (await Client.PostAsync($"{Products}/{product.Id}/options/{Guid.NewGuid()}/deactivate", null)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await Client.PostAsync($"{Categories}/{Guid.NewGuid()}/deactivate", null)).StatusCode);
    }
}

public class ReferencedOptionApiTests(ReferencedOptionsFixture fixture) : CatalogApiTestsBase(fixture), IClassFixture<ReferencedOptionsFixture>
{
    [Fact]
    public async Task Referenced_option_cannot_be_deleted_but_can_be_deactivated()
    {
        var product = await ProductAsync();
        var option = product.Options[0];

        var delete = await Client.DeleteAsync($"{Products}/{product.Id}/options/{option.Id}");
        var deactivate = await Client.PostAsync($"{Products}/{product.Id}/options/{option.Id}/deactivate", null);

        Assert.Equal(HttpStatusCode.Conflict, delete.StatusCode);
        Assert.Equal(HttpStatusCode.OK, deactivate.StatusCode);
    }
}
