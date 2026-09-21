using BuenaOnda.Application.Catalog.Categories;
using BuenaOnda.Application.Catalog.Products;
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
        services.AddScoped<CreateProduct>();
        services.AddScoped<UpdateProduct>();
        services.AddScoped<GetProduct>();
        services.AddScoped<ListProducts>();
        services.AddScoped<AddOption>();
        services.AddScoped<AddCharacteristic>();
        services.AddScoped<RemoveCharacteristic>();
        return services;
    }
}
