using BuenaOnda.Application.Catalog.Categories;
using Microsoft.Extensions.DependencyInjection;

namespace BuenaOnda.Application;

public static class DependencyInjection
{
    /// <summary>Registra los casos de uso de la aplicación.</summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<CreateCategory>();
        services.AddScoped<UpdateCategory>();
        services.AddScoped<GetCategory>();
        services.AddScoped<ListCategories>();
        return services;
    }
}
