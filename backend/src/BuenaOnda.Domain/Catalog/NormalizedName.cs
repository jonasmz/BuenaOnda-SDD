namespace BuenaOnda.Domain.Catalog;

/// <summary>
/// Normalización usada para comparar nombres y valores del catálogo (FR-011, FR-020):
/// se ignoran mayúsculas y espacios de borde; los acentos siguen siendo significativos.
/// </summary>
public static class NormalizedName
{
    public static string Normalize(string? value) =>
        (value ?? string.Empty).Trim().ToLowerInvariant();
}
