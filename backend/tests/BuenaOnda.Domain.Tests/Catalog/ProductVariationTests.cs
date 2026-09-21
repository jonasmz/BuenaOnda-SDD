using BuenaOnda.Domain.Catalog;
using BuenaOnda.Domain.Common;

namespace BuenaOnda.Domain.Tests.Catalog;

public class ProductVariationTests
{
    private static readonly Category Cat = Category.Create("Comidas", null);

    private static OptionDraft Opt(decimal price, params (string Name, string Value)[] values) =>
        new(values.ToDictionary(v => v.Name, v => v.Value), price, true);

    private static Product New(string[]? characteristics, params OptionDraft[] options) =>
        Product.Create("Producto", null, null, Cat, characteristics, options);

    [Fact]
    public void Papas_fritas_one_characteristic_two_options_with_own_price()
    {
        var product = New(["tamaño"], Opt(1000, ("tamaño", "chica")), Opt(1800, ("tamaño", "grande")));

        Assert.Equal(2, product.Options.Count);
        Assert.Equal([1000m, 1800m], product.Options.Select(o => o.Price));
    }

    [Fact]
    public void Hamburguesa_modalidad_simple_o_completa() =>
        Assert.Equal(2, New(["modalidad"], Opt(3000, ("modalidad", "simple")), Opt(4200, ("modalidad", "completa"))).Options.Count);

    [Fact]
    public void Pizza_one_option_per_variety() =>
        Assert.Equal(3, New(["variedad"], Opt(1, ("variedad", "muzzarella")), Opt(1, ("variedad", "fugazzeta")), Opt(1, ("variedad", "napolitana"))).Options.Count);

    [Fact]
    public void Bebida_by_presentation_and_agua_saborizada_by_two_characteristics()
    {
        Assert.Equal(4, New(["presentación"],
            Opt(1, ("presentación", "500 ml")), Opt(1, ("presentación", "750 ml")),
            Opt(1, ("presentación", "1 litro")), Opt(1, ("presentación", "1,5 litros"))).Options.Count);

        var agua = New(["presentación", "sabor"],
            Opt(1, ("presentación", "500 ml"), ("sabor", "pomelo")),
            Opt(1, ("presentación", "500 ml"), ("sabor", "manzana")),
            Opt(1, ("presentación", "1,5 litros"), ("sabor", "pomelo")));
        Assert.Equal(3, agua.Options.Count);
    }

    [Fact]
    public void Indistinguishable_option_is_rejected_ignoring_case_and_edge_spaces()
    {
        var product = New(["tamaño"], Opt(1, ("tamaño", "chica")));

        Assert.Throws<ConflictException>(() => product.AddOption(Opt(2, ("Tamaño", "  CHICA ")), Cat));
    }

    [Fact]
    public void Accents_are_significant_in_values() =>
        Assert.Equal(2, New(["sabor"], Opt(1, ("sabor", "limon")), Opt(1, ("sabor", "limón"))).Options.Count);

    [Fact]
    public void Repeated_characteristic_is_rejected() =>
        Assert.Throws<ConflictException>(() => New(["sabor", " SABOR "], Opt(1, ("sabor", "a"))));

    [Fact]
    public void Incomplete_or_extra_values_are_rejected()
    {
        Assert.Throws<ValidationException>(() => New(["tamaño", "sabor"], Opt(1, ("tamaño", "chica"))));
        Assert.Throws<ValidationException>(() => New(["tamaño"], Opt(1, ("tamaño", "  "))));
        Assert.Throws<ValidationException>(() => New(["tamaño"], Opt(1, ("tamaño", "chica"), ("otro", "x"))));
        Assert.Throws<ValidationException>(() => New(null, Opt(1, ("tamaño", "chica"))));
    }

    [Fact]
    public void Product_without_characteristics_needs_exactly_one_option_and_with_them_at_least_one()
    {
        Assert.Throws<ValidationException>(() => New(null, Opt(1), Opt(2)));
        Assert.Throws<ValidationException>(() => New(null));
        Assert.Throws<ValidationException>(() => New(["tamaño"]));
    }

    [Fact]
    public void Single_option_product_is_consultable_with_empty_values()
    {
        var product = New(null, Opt(500));

        Assert.Empty(product.Characteristics);
        Assert.Empty(Assert.Single(product.Options).Values);
    }

    [Fact]
    public void AddOption_adds_new_presentation()
    {
        var product = New(["presentación"], Opt(1, ("presentación", "500 ml")));

        product.AddOption(Opt(9, ("presentación", "2 litros")), Cat);

        Assert.Equal(2, product.Options.Count);
    }

    [Fact]
    public void AddCharacteristic_requires_value_for_each_existing_option_and_keeps_option_ids()
    {
        var product = New(null, Opt(500));
        var optionId = product.Options[0].Id;

        Assert.Throws<ValidationException>(() => product.AddCharacteristic("tamaño", null, Cat));
        Assert.Throws<ValidationException>(() =>
            product.AddCharacteristic("tamaño", new Dictionary<Guid, string> { [Guid.NewGuid()] = "x" }, Cat));

        product.AddCharacteristic("tamaño", new Dictionary<Guid, string> { [optionId] = "chico" }, Cat);

        Assert.Equal(optionId, Assert.Single(product.Options).Id);
        Assert.Equal("chico", product.Options[0].Values[0].Value);
        product.AddOption(Opt(900, ("tamaño", "grande")), Cat);
        Assert.Equal(2, product.Options.Count);
    }

    [Fact]
    public void AddCharacteristic_rejects_duplicate_name()
    {
        var product = New(["tamaño"], Opt(1, ("tamaño", "chica")));

        Assert.Throws<ConflictException>(() => product.AddCharacteristic("TAMAÑO", new Dictionary<Guid, string> { [product.Options[0].Id] = "x" }, Cat));
    }

    [Fact]
    public void RemoveCharacteristic_only_when_options_stay_distinguishable()
    {
        var product = New(["presentación", "sabor"],
            Opt(1, ("presentación", "500 ml"), ("sabor", "pomelo")),
            Opt(1, ("presentación", "500 ml"), ("sabor", "manzana")));
        var presentacion = product.Characteristics.First(c => c.Name == "presentación");
        var sabor = product.Characteristics.First(c => c.Name == "sabor");

        Assert.Throws<ConflictException>(() => product.RemoveCharacteristic(sabor.Id, Cat));
        Assert.Equal(2, product.Characteristics.Count);
    }

    [Fact]
    public void RemoveCharacteristic_from_single_option_product_leaves_empty_values()
    {
        var product = New(["tamaño"], Opt(1, ("tamaño", "chica")));

        product.RemoveCharacteristic(product.Characteristics[0].Id, Cat);

        Assert.Empty(product.Characteristics);
        Assert.Empty(product.Options[0].Values);
        Assert.Throws<NotFoundException>(() => product.RemoveCharacteristic(Guid.NewGuid(), Cat));
    }
}
