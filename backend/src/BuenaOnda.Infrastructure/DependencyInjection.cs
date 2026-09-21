using BuenaOnda.Application.Catalog.Ports;
using BuenaOnda.Infrastructure.Persistence;
using BuenaOnda.Infrastructure.Persistence.Categories;
using BuenaOnda.Infrastructure.Persistence.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BuenaOnda.Infrastructure;

public static class DependencyInjection
{
    /// <summary>Registra los adaptadores de salida (persistencia con EF Core y PostgreSQL).</summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<CatalogDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        return services;
    }

    /// <summary>
    /// Aplica las migraciones pendientes de EF Core. Se usa al arrancar en desarrollo, para que una base
    /// nueva (por ejemplo la del contenedor de Docker Compose) quede lista sin pasos manuales.
    /// </summary>
    public static async Task ApplyMigrationsAsync(this IServiceProvider services, CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        await dbContext.Database.MigrateAsync(cancellationToken);
    }
}
