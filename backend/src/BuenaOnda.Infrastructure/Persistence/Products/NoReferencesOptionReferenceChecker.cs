using BuenaOnda.Application.Catalog.Ports;

namespace BuenaOnda.Infrastructure.Persistence.Products;

/// <summary>Implementación por defecto: aún no existen funcionalidades consumidoras, ninguna opción está referenciada.</summary>
internal sealed class NoReferencesOptionReferenceChecker : IOptionReferenceChecker
{
    public Task<bool> IsReferencedAsync(Guid optionId, CancellationToken cancellationToken = default) =>
        Task.FromResult(false);
}
