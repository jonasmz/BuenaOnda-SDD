using BuenaOnda.Api;
using BuenaOnda.Application;
using BuenaOnda.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Catalog")
    ?? throw new InvalidOperationException("Falta la cadena de conexión 'ConnectionStrings:Catalog'.");

builder.Services.AddApplication();
builder.Services.AddInfrastructure(connectionString);
builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<DomainExceptionHandler>();
builder.Services.AddOpenApi();

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    // Solo en desarrollo: una base nueva queda migrada al arrancar; en otros entornos las migraciones
    // se aplican de forma explícita.
    await app.Services.ApplyMigrationsAsync();
    app.MapOpenApi();
}

app.MapControllers();

app.Run();

// Necesario para las pruebas de integración (WebApplicationFactory).
public partial class Program;
