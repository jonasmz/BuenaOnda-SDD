using BuenaOnda.Domain.Common;

namespace BuenaOnda.Domain.Catalog;

/// <summary>Categoría del catálogo. Su nombre es único sin distinguir mayúsculas ni espacios de borde (FR-020).</summary>
public sealed class Category
{
    private Category()
    {
        Name = string.Empty;
        NormalizedName = string.Empty;
    }

    public Guid Id { get; private set; }

    public string Name { get; private set; }

    /// <summary>Nombre normalizado, persistido para reforzar la unicidad.</summary>
    public string NormalizedName { get; private set; }

    public string? Description { get; private set; }

    public bool IsActive { get; private set; }

    public static Category Create(string? name, string? description)
    {
        var category = new Category { Id = Guid.NewGuid(), IsActive = true };
        category.Update(name, description);
        return category;
    }

    public void Update(string? name, string? description)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ValidationException("El nombre de la categoría es obligatorio.");
        }

        Name = name.Trim();
        NormalizedName = Catalog.NormalizedName.Normalize(Name);
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
    }

}
