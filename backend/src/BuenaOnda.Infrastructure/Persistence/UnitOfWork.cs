using BuenaOnda.Application.Catalog.Ports;
using BuenaOnda.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace BuenaOnda.Infrastructure.Persistence;

internal sealed class UnitOfWork(CatalogDbContext dbContext) : IUnitOfWork
{
    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            // Carrera entre dos altas: el índice único es la garantía final de FR-011 y FR-020.
            throw new ConflictException("Ya existe un registro con ese nombre o esos valores.");
        }
    }
}
