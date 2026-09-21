namespace BuenaOnda.Domain.Catalog;

/// <summary>Valor de texto que una opción toma para una característica de su producto.</summary>
public sealed class OptionValue
{
    private OptionValue()
    {
        Value = string.Empty;
        NormalizedValue = string.Empty;
    }

    internal OptionValue(Guid characteristicId, string value)
    {
        CharacteristicId = characteristicId;
        Value = value.Trim();
        NormalizedValue = Catalog.NormalizedName.Normalize(value);
    }

    internal void Change(string value)
    {
        Value = value.Trim();
        NormalizedValue = Catalog.NormalizedName.Normalize(value);
    }

    public Guid CharacteristicId { get; private set; }

    public string Value { get; private set; }

    public string NormalizedValue { get; private set; }
}
