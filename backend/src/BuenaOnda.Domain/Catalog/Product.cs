using BuenaOnda.Domain.Common;

namespace BuenaOnda.Domain.Catalog;

/// <summary>Producto del catálogo: pertenece a una categoría y contiene sus opciones comercializables.</summary>
public sealed class Product
{
    private readonly List<SellableOption> _options = [];

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

    public Guid CategoryId { get; private set; }

    public bool IsActive { get; private set; }

    public IReadOnlyList<SellableOption> Options => _options;

    public bool HasImage => ImageUrl is not null;

    /// <summary>Crea un producto sin características de variación: tiene exactamente una opción (invariante 1).</summary>
    public static Product Create(
        string? name,
        string? description,
        string? imageUrl,
        Category category,
        decimal? price,
        bool? isMarkedAvailable,
        string? optionDescription = null,
        string? optionImageUrl = null)
    {
        if (!category.IsActive)
        {
            throw new ConflictException("No se puede asignar un producto a una categoría inactiva.");
        }

        var product = new Product { Id = Guid.NewGuid(), IsActive = true, CategoryId = category.Id };
        product.SetInformation(name, description, imageUrl);
        product._options.Add(SellableOption.Create(price, isMarkedAvailable, optionDescription, optionImageUrl));
        return product;
    }

    /// <summary>Modifica la información comercial; el producto y su categoría actual deben estar activos (invariante 7).</summary>
    public void Update(string? name, string? description, string? imageUrl, Category currentCategory, Category targetCategory)
    {
        if (!IsActive || !currentCategory.IsActive)
        {
            throw new ConflictException("Un producto inactivo o de una categoría inactiva no admite modificaciones.");
        }

        if (targetCategory.Id != CategoryId && !targetCategory.IsActive)
        {
            throw new ConflictException("No se puede asignar un producto a una categoría inactiva.");
        }

        SetInformation(name, description, imageUrl);
        CategoryId = targetCategory.Id;
    }

    /// <summary>Disponible si alguna de sus opciones tiene disponibilidad efectiva.</summary>
    public bool IsAvailable(Category category) => _options.Any(o => o.IsAvailable(this, category));

    /// <summary>Visible al público: producto activo, categoría activa y con imagen.</summary>
    public bool IsVisibleToPublic(Category category) => IsActive && category.IsActive && HasImage;

    private void SetInformation(string? name, string? description, string? imageUrl)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ValidationException("El nombre del producto es obligatorio.");
        }

        Name = name.Trim();
        NormalizedName = Catalog.NormalizedName.Normalize(Name);
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        ImageUrl = ImageReference.Normalize(imageUrl);
    }

}
