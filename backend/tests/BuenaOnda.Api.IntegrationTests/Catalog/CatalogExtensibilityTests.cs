using System.Net;

namespace BuenaOnda.Api.IntegrationTests.Catalog;

/// <summary>US4: un tipo de producto nuevo se incorpora con los conceptos existentes (FR-012, SC-005).</summary>
public class CatalogExtensibilityTests(CatalogApiFixture fixture) : CatalogApiTestsBase(fixture), IClassFixture<CatalogApiFixture>
{
    [Fact]
    public async Task A_new_category_with_new_kinds_of_products_needs_no_model_change()
    {
        var tragos = await CategoryAsync(Unique("Tragos"));

        var fernet = await PostProductAsync(tragos, "Fernet con cola", price: 4500m);
        var gin = await PostProductAsync(tragos, "Gin tonic", price: 5200m, imageUrl: "https://cdn.example.com/gin.png");

        Assert.Equal(HttpStatusCode.Created, fernet.StatusCode);
        Assert.Equal(HttpStatusCode.Created, gin.StatusCode);
    }
}
