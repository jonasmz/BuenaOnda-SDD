namespace BuenaOnda.Domain.Catalog;

/// <summary>Datos de una opción por crear; los valores se indican por nombre de característica.</summary>
public sealed record OptionDraft(
    IReadOnlyDictionary<string, string>? Values,
    decimal? Price,
    bool? IsMarkedAvailable,
    string? Description = null,
    string? ImageUrl = null);
