using BuenaOnda.Domain.Common;

namespace BuenaOnda.Domain.Catalog;

/// <summary>Opción comercializable de un producto: lo que realmente se vende, con su precio propio.</summary>
public sealed class SellableOption
{
    private SellableOption()
    {
    }

    public Guid Id { get; private set; }

    public decimal Price { get; private set; }

    /// <summary>Marca manual de disponibilidad; la baja y la reactivación no la modifican.</summary>
    public bool IsMarkedAvailable { get; private set; }

    public bool IsActive { get; private set; }

    public string? Description { get; private set; }

    /// <summary>Imagen propia, informativa: no condiciona la visibilidad del producto.</summary>
    public string? ImageUrl { get; private set; }

    internal static SellableOption Create(decimal? price, bool? isMarkedAvailable, string? description, string? imageUrl)
    {
        var option = new SellableOption { Id = Guid.NewGuid(), IsActive = true };
        option.IsMarkedAvailable = isMarkedAvailable
            ?? throw new ValidationException("La disponibilidad de la opción es obligatoria.");
        option.Update(price, description, imageUrl);
        return option;
    }

    internal void Update(decimal? price, string? description, string? imageUrl)
    {
        if (price is null or < 0)
        {
            throw new ValidationException("El precio es obligatorio y no puede ser negativo.");
        }

        Price = price.Value;
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        ImageUrl = ImageReference.Normalize(imageUrl);
    }

    /// <summary>Disponibilidad efectiva: marca manual, opción activa, producto activo y categoría activa.</summary>
    public bool IsAvailable(Product product, Category category) =>
        IsMarkedAvailable && IsActive && product.IsActive && category.IsActive;

    /// <summary>Visible al público: opción activa y producto visible al público.</summary>
    public bool IsVisibleToPublic(Product product, Category category) =>
        IsActive && product.IsVisibleToPublic(category);
}
