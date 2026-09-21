using BuenaOnda.Domain.Common;

namespace BuenaOnda.Domain.Catalog;

/// <summary>Las imágenes son referencias (URL) ilustrativas; no hay mecanismo de carga (research R-9).</summary>
internal static class ImageReference
{
    public static string? Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmed = value.Trim();
        if (!Uri.TryCreate(trimmed, UriKind.Absolute, out var uri) || uri.Scheme is not ("http" or "https"))
        {
            throw new ValidationException("La imagen debe ser una URL http o https válida.");
        }

        return trimmed;
    }
}
