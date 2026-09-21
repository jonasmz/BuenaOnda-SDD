using BuenaOnda.Domain.Common;

namespace BuenaOnda.Domain.Catalog;

/// <summary>
/// Producto del catálogo plano: pertenece a una categoría y lleva su propio precio y su marca manual de
/// disponibilidad. Cada presentación o variedad que se vende por separado es un producto distinto.
/// </summary>
public sealed class Product
{
    private Product()
    {
        Name = string.Empty;
        NormalizedName = string.Empty;
    }

    public Guid Id { get; private set; }

    public string Name { get; private set; }

    /// <summary>Nombre normalizado; único dentro de la categoría.</summary>
    public string NormalizedName { get; private set; }

    public string? Description { get; private set; }

    public string? ImageUrl { get; private set; }

    public decimal Price { get; private set; }

    /// <summary>Marca manual de disponibilidad; la baja y la reactivación no la modifican.</summary>
    public bool IsMarkedAvailable { get; private set; }

    public Guid CategoryId { get; private set; }

    public bool IsActive { get; private set; }

    public bool HasImage => ImageUrl is not null;

    public static Product Create(
        string? name, string? description, string? imageUrl, Category category, decimal? price, bool? isMarkedAvailable)
    {
        if (!category.IsActive)
        {
            throw new ConflictException("No se puede asignar un producto a una categoría inactiva.");
        }

        var product = new Product
        {
            Id = Guid.NewGuid(),
            IsActive = true,
            CategoryId = category.Id,
            IsMarkedAvailable = isMarkedAvailable
                ?? throw new ValidationException("La disponibilidad del producto es obligatoria."),
        };
        product.SetInformation(name, description, imageUrl, price);
        return product;
    }

    /// <summary>Modifica la información comercial y el precio; el producto y su categoría actual deben estar activos.</summary>
    public void Update(
        string? name, string? description, string? imageUrl, decimal? price, Category currentCategory, Category targetCategory)
    {
        EnsureModifiable(currentCategory);
        if (targetCategory.Id != CategoryId && !targetCategory.IsActive)
        {
            throw new ConflictException("No se puede asignar un producto a una categoría inactiva.");
        }

        SetInformation(name, description, imageUrl, price);
        CategoryId = targetCategory.Id;
    }

    /// <summary>Fija la marca manual de disponibilidad (FR-010).</summary>
    public void SetAvailability(bool? isMarkedAvailable, Category category)
    {
        EnsureModifiable(category);
        IsMarkedAvailable = isMarkedAvailable
            ?? throw new ValidationException("La disponibilidad del producto es obligatoria.");
    }

    /// <summary>Baja reversible; no altera la marca manual de disponibilidad.</summary>
    public void Deactivate() => IsActive = false;

    public void Reactivate() => IsActive = true;

    /// <summary>Disponibilidad efectiva: marca manual, producto activo y categoría activa.</summary>
    public bool IsAvailable(Category category) => IsMarkedAvailable && IsActive && category.IsActive;

    /// <summary>Visible al público: producto activo, categoría activa y con imagen.</summary>
    public bool IsVisibleToPublic(Category category) => IsActive && category.IsActive && HasImage;

    private void EnsureModifiable(Category category)
    {
        if (!IsActive || !category.IsActive)
        {
            throw new ConflictException("Un producto inactivo o de una categoría inactiva no admite modificaciones.");
        }
    }

    /// <summary>Valida todo antes de asignar, para no dejar el producto a medias si algo falla.</summary>
    private void SetInformation(string? name, string? description, string? imageUrl, decimal? priceValue)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ValidationException("El nombre del producto es obligatorio.");
        }

        if (priceValue is null or < 0)
        {
            throw new ValidationException("El precio es obligatorio y no puede ser negativo.");
        }

        var image = ImageReference.Normalize(imageUrl);
        Name = name.Trim();
        NormalizedName = Catalog.NormalizedName.Normalize(Name);
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        ImageUrl = image;
        Price = priceValue.Value;
    }
}
