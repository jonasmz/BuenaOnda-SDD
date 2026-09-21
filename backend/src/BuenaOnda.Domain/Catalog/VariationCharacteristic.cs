using BuenaOnda.Domain.Common;

namespace BuenaOnda.Domain.Catalog;

/// <summary>Característica de variación de un producto (por ejemplo presentación o sabor); no existe catálogo global.</summary>
public sealed class VariationCharacteristic
{
    private VariationCharacteristic()
    {
        Name = string.Empty;
        NormalizedName = string.Empty;
    }

    public Guid Id { get; private set; }

    public string Name { get; private set; }

    public string NormalizedName { get; private set; }

    internal static VariationCharacteristic Create(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ValidationException("El nombre de la característica es obligatorio.");
        }

        return new VariationCharacteristic
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            NormalizedName = Catalog.NormalizedName.Normalize(name),
        };
    }
}
