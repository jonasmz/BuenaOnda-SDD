using System.Net;
using System.Net.Http.Json;

namespace BuenaOnda.Api.IntegrationTests.Catalog;

public class ProductVariationApiTests(CatalogApiFixture fixture) : IClassFixture<CatalogApiFixture>
{
    private const string Products = "/api/admin/catalog/products";

    private record IdDto(Guid Id);
    private record CharacteristicDto(Guid Id, string Name);
    private record OptionDto(Guid Id, Dictionary<string, string> Values, decimal Price, bool IsAvailable);
    private record ProductDto(Guid Id, bool IsAvailable, List<CharacteristicDto> Characteristics, List<OptionDto> Options);

    private static string Unique(string prefix) => $"{prefix}-{Guid.NewGuid():N}";

    private async Task<Guid> CategoryAsync() =>
        (await (await fixture.CreateClient().PostAsJsonAsync("/api/admin/catalog/categories", new { name = Unique("C") }))
            .Content.ReadFromJsonAsync<IdDto>())!.Id;

    private static object Opt(decimal price, Dictionary<string, string>? values = null) =>
        new { values = values ?? [], price, isMarkedAvailable = true };

    private async Task<ProductDto> CreateAsync(string[]? characteristics, params object[] options)
    {
        var response = await fixture.CreateClient().PostAsJsonAsync(Products,
            new { name = Unique("P"), categoryId = await CategoryAsync(), characteristics, options });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<ProductDto>())!;
    }

    [Fact]
    public async Task Create_with_characteristics_and_options_then_get()
    {
        var created = await CreateAsync(["presentación", "sabor"],
            Opt(1, new() { ["presentación"] = "500 ml", ["sabor"] = "pomelo" }),
            Opt(2, new() { ["presentación"] = "1,5 litros", ["sabor"] = "pomelo" }));

        var fetched = (await fixture.CreateClient().GetFromJsonAsync<ProductDto>($"{Products}/{created.Id}"))!;

        Assert.Equal(2, fetched.Characteristics.Count);
        Assert.Equal(2, fetched.Options.Count);
        Assert.Contains(fetched.Options, o => o.Values["presentación"] == "1,5 litros" && o.Price == 2);
    }

    [Fact]
    public async Task Water_presentations_and_a_new_one_can_be_added()
    {
        var created = await CreateAsync(["presentación"],
            Opt(1, new() { ["presentación"] = "500 ml" }), Opt(1, new() { ["presentación"] = "750 ml" }),
            Opt(1, new() { ["presentación"] = "1 litro" }), Opt(1, new() { ["presentación"] = "1,5 litros" }));

        var response = await fixture.CreateClient().PostAsJsonAsync($"{Products}/{created.Id}/options",
            Opt(5, new() { ["presentación"] = "2 litros" }));
        var product = (await response.Content.ReadFromJsonAsync<ProductDto>())!;

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal(5, product.Options.Count);
    }

    [Fact]
    public async Task Duplicate_option_and_duplicate_characteristic_are_conflicts()
    {
        var created = await CreateAsync(["tamaño"], Opt(1, new() { ["tamaño"] = "chica" }));
        var client = fixture.CreateClient();

        var duplicateOption = await client.PostAsJsonAsync($"{Products}/{created.Id}/options", Opt(9, new() { ["tamaño"] = " CHICA " }));
        var duplicateCharacteristic = await client.PostAsJsonAsync($"{Products}/{created.Id}/characteristics",
            new { name = "Tamaño", valuesForExistingOptions = new Dictionary<Guid, string> { [created.Options[0].Id] = "x" } });

        Assert.Equal(HttpStatusCode.Conflict, duplicateOption.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, duplicateCharacteristic.StatusCode);
    }

    [Fact]
    public async Task Incomplete_values_are_bad_request()
    {
        var created = await CreateAsync(["tamaño", "sabor"], Opt(1, new() { ["tamaño"] = "chica", ["sabor"] = "x" }));

        var response = await fixture.CreateClient().PostAsJsonAsync($"{Products}/{created.Id}/options", Opt(2, new() { ["tamaño"] = "grande" }));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Add_then_remove_characteristic_keeping_option_ids()
    {
        var created = await CreateAsync(null, Opt(500));
        var optionId = created.Options[0].Id;
        var client = fixture.CreateClient();

        var added = await client.PostAsJsonAsync($"{Products}/{created.Id}/characteristics",
            new { name = "tamaño", valuesForExistingOptions = new Dictionary<Guid, string> { [optionId] = "chico" } });
        var afterAdd = (await added.Content.ReadFromJsonAsync<ProductDto>())!;
        var second = await client.PostAsJsonAsync($"{Products}/{created.Id}/options", Opt(900, new() { ["tamaño"] = "grande" }));
        var removeWithTwo = await client.DeleteAsync($"{Products}/{created.Id}/characteristics/{afterAdd.Characteristics[0].Id}");

        Assert.Equal(HttpStatusCode.Created, added.StatusCode);
        Assert.Equal(optionId, afterAdd.Options[0].Id);
        Assert.Equal(HttpStatusCode.Created, second.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, removeWithTwo.StatusCode);
    }

    [Fact]
    public async Task Remove_characteristic_from_single_option_product_succeeds()
    {
        var created = await CreateAsync(["tamaño"], Opt(1, new() { ["tamaño"] = "chica" }));

        var response = await fixture.CreateClient().DeleteAsync($"{Products}/{created.Id}/characteristics/{created.Characteristics[0].Id}");
        var product = (await response.Content.ReadFromJsonAsync<ProductDto>())!;

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Empty(product.Characteristics);
        Assert.Empty(product.Options[0].Values);
    }
}
