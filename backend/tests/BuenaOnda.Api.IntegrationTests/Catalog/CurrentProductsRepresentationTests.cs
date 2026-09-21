using System.Net;
using System.Net.Http.Json;

namespace BuenaOnda.Api.IntegrationTests.Catalog;

/// <summary>Los productos actuales (Requerimientos §9) se representan como productos planos (SC-001, SC-002).</summary>
public class CurrentProductsRepresentationTests(CatalogApiFixture fixture) : CatalogApiTestsBase(fixture), IClassFixture<CatalogApiFixture>
{
    public static TheoryData<string, string[]> Catalog => new()
    {
        { "Comidas", ["Papas fritas chicas", "Papas fritas grandes", "Milanesa simple", "Milanesa completa",
                      "Hamburguesa simple", "Hamburguesa completa", "Pizza muzzarella", "Pizza fugazzeta", "Pizza napolitana"] },
        { "Bebidas", ["Coca-Cola 500 ml", "Coca-Cola 1,5 litros", "Sprite 500 ml", "Sprite 1,5 litros",
                      "Agua mineral 500 ml", "Cerveza 1 litro",
                      "Agua saborizada pomelo 500 ml", "Agua saborizada manzana 1,5 litros"] },
    };

    [Theory]
    [MemberData(nameof(Catalog))]
    public async Task Every_current_product_is_a_plain_product_with_its_own_price(string category, string[] names)
    {
        var categoryId = await CategoryAsync(Unique(category));

        var price = 1000m;
        foreach (var name in names)
        {
            Assert.Equal(HttpStatusCode.Created, (await PostProductAsync(categoryId, name, price: price)).StatusCode);
            price += 250m;
        }
    }

    [Fact]
    public async Task Each_size_of_the_same_dish_is_independent()
    {
        var categoryId = await CategoryAsync();
        var small = await ProductAsync(categoryId, "Papas fritas chicas");
        var large = await ProductAsync(categoryId, "Papas fritas grandes");

        Assert.NotEqual(small.Id, large.Id);
        Assert.Equal(HttpStatusCode.OK, (await Client.PutAsync($"{Products}/{small.Id}/availability",
            JsonContent.Create(new { isMarkedAvailable = false }))).StatusCode);
        Assert.True((await GetAsync(large.Id)).IsAvailable);
    }
}
