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
        services.AddScoped<IOptionReferenceChecker, NoReferencesOptionReferenceChecker>();
        return services;
    }
}
