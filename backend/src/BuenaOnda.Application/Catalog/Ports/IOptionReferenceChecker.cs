namespace BuenaOnda.Application.Catalog.Ports;

/// <summary>
/// Puerto de salida que informa si alguna funcionalidad consumidora (POS, recetas, inventario)
/// referenció una opción. Cada feature que referencie opciones DEBE extender su implementación
/// (research R-13); de lo contrario podrían eliminarse opciones referenciadas.
/// </summary>
public interface IOptionReferenceChecker
{
    Task<bool> IsReferencedAsync(Guid optionId, CancellationToken cancellationToken = default);
}
