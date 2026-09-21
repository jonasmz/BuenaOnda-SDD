using System.Net;
using System.Net.Http.Json;

namespace BuenaOnda.Api.IntegrationTests.Catalog;

public class CatalogModificationApiTests(CatalogApiFixture fixture) : CatalogApiTestsBase(fixture), IClassFixture<CatalogApiFixture>
{
    private Task<HttpResponseMessage> SetAvailabilityAsync(Guid id, bool? value) =>
        Client.PutAsJsonAsync($"{Products}/{id}/availability", new { isMarkedAvailable = value });

    [Fact]
    public async Task Availability_is_set_per_product_and_is_required()
    {
        var product = await ProductAsync();

        var off = (await (await SetAvailabilityAsync(product.Id, false)).Content.ReadFromJsonAsync<ProductDto>())!;
        var on = (await (await SetAvailabilityAsync(product.Id, true)).Content.ReadFromJsonAsync<ProductDto>())!;

        Assert.False(off.IsAvailable);
        Assert.True(on.IsAvailable);
        Assert.Equal(HttpStatusCode.BadRequest, (await SetAvailabilityAsync(product.Id, null)).StatusCode);
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
        var unavailable = await ProductAsync(categoryId);
        var available = await ProductAsync(categoryId);
        await SetAvailabilityAsync(unavailable.Id, false);

        var deactivated = await Client.PostAsync($"{Categories}/{categoryId}/deactivate", null);
        var whileInactive = await GetAsync(available.Id);
        await Client.PostAsync($"{Categories}/{categoryId}/reactivate", null);

        Assert.False((await deactivated.Content.ReadFromJsonAsync<CategoryDto>())!.IsActive);
        Assert.False(whileInactive.IsAvailable);
        Assert.False(whileInactive.IsVisibleToPublic);
        Assert.True((await GetAsync(available.Id)).IsAvailable);
        Assert.True((await GetAsync(available.Id)).IsVisibleToPublic);
        Assert.False((await GetAsync(unavailable.Id)).IsMarkedAvailable);
        Assert.False((await GetAsync(unavailable.Id)).IsAvailable);
    }

    [Fact]
    public async Task Product_deactivation_is_reversible_keeps_the_mark_and_blocks_modifications()
    {
        var categoryId = await CategoryAsync();
        var product = await ProductAsync(categoryId);
        await SetAvailabilityAsync(product.Id, false);

        var deactivated = (await (await Client.PostAsync($"{Products}/{product.Id}/deactivate", null)).Content.ReadFromJsonAsync<ProductDto>())!;
        var blockedMark = await SetAvailabilityAsync(product.Id, true);
        var blockedUpdate = await Client.PutAsJsonAsync($"{Products}/{product.Id}", new { name = product.Name, categoryId, price = 5m });
        var reactivated = (await (await Client.PostAsync($"{Products}/{product.Id}/reactivate", null)).Content.ReadFromJsonAsync<ProductDto>())!;

        Assert.False(deactivated.IsActive);
        Assert.False(deactivated.IsVisibleToPublic);
        Assert.Equal(HttpStatusCode.Conflict, blockedMark.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, blockedUpdate.StatusCode);
        Assert.True(reactivated.IsActive);
        Assert.False(reactivated.IsMarkedAvailable);
    }

    [Fact]
    public async Task There_are_no_endpoints_to_delete_products_or_categories()
    {
        var product = await ProductAsync();

        Assert.Equal(HttpStatusCode.MethodNotAllowed, (await Client.DeleteAsync($"{Products}/{product.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.MethodNotAllowed, (await Client.DeleteAsync($"{Categories}/{product.CategoryId}")).StatusCode);
    }

    [Fact]
    public async Task Unknown_ids_are_not_found()
    {
        Assert.Equal(HttpStatusCode.NotFound, (await Client.PostAsync($"{Products}/{Guid.NewGuid()}/deactivate", null)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await SetAvailabilityAsync(Guid.NewGuid(), true)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await Client.PostAsync($"{Categories}/{Guid.NewGuid()}/deactivate", null)).StatusCode);
    }
}
