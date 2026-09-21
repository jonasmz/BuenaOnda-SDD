namespace BuenaOnda.Application.Catalog.Ports;

/// <summary>Puerto de salida que confirma de forma atómica los cambios de un caso de uso.</summary>
public interface IUnitOfWork
{
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
