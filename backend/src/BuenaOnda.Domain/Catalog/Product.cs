using BuenaOnda.Domain.Common;

namespace BuenaOnda.Domain.Catalog;

/// <summary>
/// Producto del catálogo: raíz del agregado que controla su categoría, sus características de
/// variación y sus opciones comercializables, y garantiza los invariantes 1 a 5 y 7.
/// </summary>
public sealed class Product
{
    private readonly List<SellableOption> _options = [];
    private readonly List<VariationCharacteristic> _characteristics = [];

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

    public IReadOnlyList<VariationCharacteristic> Characteristics => _characteristics;

    public bool HasImage => ImageUrl is not null;

    /// <summary>Crea un producto sin características de variación: tiene exactamente una opción (invariante 1).</summary>
    public static Product Create(
        string? name, string? description, string? imageUrl, Category category,
        decimal? price, bool? isMarkedAvailable, string? optionDescription = null, string? optionImageUrl = null) =>
        Create(name, description, imageUrl, category, null,
            [new OptionDraft(null, price, isMarkedAvailable, optionDescription, optionImageUrl)]);

    /// <summary>Crea un producto con sus características y opciones (invariantes 1 a 4).</summary>
    public static Product Create(
        string? name, string? description, string? imageUrl, Category category,
        IReadOnlyList<string>? characteristicNames, IReadOnlyList<OptionDraft>? options)
    {
        if (!category.IsActive)
        {
            throw new ConflictException("No se puede asignar un producto a una categoría inactiva.");
        }

        var product = new Product { Id = Guid.NewGuid(), IsActive = true, CategoryId = category.Id };
        product.SetInformation(name, description, imageUrl);

        foreach (var characteristicName in characteristicNames ?? [])
        {
            product.AddCharacteristicName(characteristicName);
        }

        if (options is null || options.Count == 0)
        {
            throw new ValidationException("El producto necesita al menos una opción con su precio.");
        }

        if (product._characteristics.Count == 0 && options.Count != 1)
        {
            throw new ValidationException("Un producto sin características tiene exactamente una opción.");
        }

        foreach (var draft in options)
        {
            product.AppendOption(draft);
        }

        return product;
    }

    /// <summary>Modifica la información comercial; el producto y su categoría actual deben estar activos (invariante 7).</summary>
    public void Update(string? name, string? description, string? imageUrl, Category currentCategory, Category targetCategory)
    {
        EnsureModifiable(currentCategory);
        if (targetCategory.Id != CategoryId && !targetCategory.IsActive)
        {
            throw new ConflictException("No se puede asignar un producto a una categoría inactiva.");
        }

        SetInformation(name, description, imageUrl);
        CategoryId = targetCategory.Id;
    }

    /// <summary>Agrega una opción comercializable distinguible de las existentes (FR-010, FR-011).</summary>
    public SellableOption AddOption(OptionDraft draft, Category category)
    {
        EnsureModifiable(category);
        return AppendOption(draft);
    }

    /// <summary>Agrega una característica aportando su valor para cada opción existente (invariante 5).</summary>
    public VariationCharacteristic AddCharacteristic(
        string? name, IReadOnlyDictionary<Guid, string>? valuesForExistingOptions, Category category)
    {
        EnsureModifiable(category);
        var provided = valuesForExistingOptions ?? new Dictionary<Guid, string>();
        if (provided.Keys.Any(id => _options.All(o => o.Id != id)))
        {
            throw new ValidationException("Se indicó un valor para una opción que no pertenece al producto.");
        }

        if (_options.Any(o => !provided.TryGetValue(o.Id, out var v) || string.IsNullOrWhiteSpace(v)))
        {
            throw new ValidationException("Debe indicarse un valor no vacío de la nueva característica para cada opción existente.");
        }

        var characteristic = AddCharacteristicName(name);
        foreach (var option in _options)
        {
            option.AddValue(new OptionValue(characteristic.Id, provided[option.Id]));
        }

        ResignAllAndCheckDistinct();
        return characteristic;
    }

    /// <summary>Quita una característica solo si las opciones restantes siguen siendo distinguibles.</summary>
    public void RemoveCharacteristic(Guid characteristicId, Category category)
    {
        EnsureModifiable(category);
        var characteristic = _characteristics.FirstOrDefault(c => c.Id == characteristicId)
            ?? throw new NotFoundException($"No existe la característica {characteristicId} en el producto.");

        var remaining = _options.Select(o => o.SignatureWithout(_characteristics, characteristicId)).ToList();
        if (remaining.Distinct().Count() != remaining.Count)
        {
            throw new ConflictException("Las opciones del producto dejarían de ser distinguibles entre sí.");
        }

        _characteristics.Remove(characteristic);
        foreach (var option in _options)
        {
            option.DropValue(characteristicId);
            option.Resign(_characteristics);
        }
    }

    /// <summary>Disponible si alguna de sus opciones tiene disponibilidad efectiva.</summary>
    public bool IsAvailable(Category category) => _options.Any(o => o.IsAvailable(this, category));

    /// <summary>Visible al público: producto activo, categoría activa y con imagen.</summary>
    public bool IsVisibleToPublic(Category category) => IsActive && category.IsActive && HasImage;

    private void EnsureModifiable(Category category)
    {
        if (!IsActive || !category.IsActive)
        {
            throw new ConflictException("Un producto inactivo o de una categoría inactiva no admite modificaciones.");
        }
    }

    private VariationCharacteristic AddCharacteristicName(string? name)
    {
        var characteristic = VariationCharacteristic.Create(name);
        if (_characteristics.Any(c => c.NormalizedName == characteristic.NormalizedName))
        {
            throw new ConflictException($"El producto ya tiene una característica llamada '{characteristic.Name}'.");
        }

        _characteristics.Add(characteristic);
        return characteristic;
    }

    private SellableOption AppendOption(OptionDraft draft)
    {
        var values = ResolveValues(draft.Values);
        var option = SellableOption.Create(
            values, _characteristics, draft.Price, draft.IsMarkedAvailable, draft.Description, draft.ImageUrl);
        if (_options.Any(o => o.Signature == option.Signature))
        {
            throw new ConflictException("Ya existe una opción del producto con esos mismos valores (aunque esté inactiva).");
        }

        _options.Add(option);
        return option;
    }

    /// <summary>Exige un valor no vacío para cada característica y ninguno adicional (invariante 2).</summary>
    private List<OptionValue> ResolveValues(IReadOnlyDictionary<string, string>? valuesByName)
    {
        var byName = new Dictionary<string, string>();
        foreach (var (key, value) in valuesByName ?? new Dictionary<string, string>())
        {
            if (!byName.TryAdd(Catalog.NormalizedName.Normalize(key), value))
            {
                throw new ValidationException($"La característica '{key}' se indicó más de una vez.");
            }
        }

        if (byName.Keys.Any(k => _characteristics.All(c => c.NormalizedName != k)))
        {
            throw new ValidationException("Se indicó un valor para una característica que el producto no tiene.");
        }

        var values = new List<OptionValue>();
        foreach (var characteristic in _characteristics)
        {
            if (!byName.TryGetValue(characteristic.NormalizedName, out var value) || string.IsNullOrWhiteSpace(value))
            {
                throw new ValidationException($"Falta el valor de la característica '{characteristic.Name}'.");
            }

            values.Add(new OptionValue(characteristic.Id, value));
        }

        return values;
    }

    private void ResignAllAndCheckDistinct()
    {
        foreach (var option in _options)
        {
            option.Resign(_characteristics);
        }

        if (_options.Select(o => o.Signature).Distinct().Count() != _options.Count)
        {
            throw new ConflictException("Las opciones del producto dejarían de ser distinguibles entre sí.");
        }
    }

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
