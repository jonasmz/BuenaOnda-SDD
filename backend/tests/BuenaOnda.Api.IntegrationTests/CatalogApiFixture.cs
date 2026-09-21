using BuenaOnda.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Xunit;

namespace BuenaOnda.Api.IntegrationTests;

/// <summary>
/// Levanta la API contra una base de datos dedicada, creada dentro de la instancia PostgreSQL 17 del
/// contenedor <c>psql-17</c>, y la elimina al terminar. La conexión administrativa se toma de la
/// variable de entorno <c>BUENAONDA_TEST_CONNECTION</c> (por ejemplo
/// <c>Host=localhost;Port=5432;Username=...;Password=...</c>), sin credenciales versionadas.
/// </summary>
public class CatalogApiFixture : WebApplicationFactory<Program>, IAsyncLifetime
{
    private const string AdminConnectionVariable = "BUENAONDA_TEST_CONNECTION";

    private readonly string _databaseName = $"buenaonda_tests_{Guid.NewGuid():N}";
    private string _adminConnection = string.Empty;

    /// <summary>Permite a cada prueba reemplazar servicios registrados, por ejemplo <c>IOptionReferenceChecker</c>.</summary>
    protected virtual void ConfigureTestServices(IServiceCollection services)
    {
    }

    public async Task InitializeAsync()
    {
        _adminConnection = Environment.GetEnvironmentVariable(AdminConnectionVariable)
            ?? throw new InvalidOperationException(
                $"Defina la variable de entorno {AdminConnectionVariable} apuntando a la instancia psql-17.");

        await using var connection = new NpgsqlConnection(_adminConnection);
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = $"CREATE DATABASE \"{_databaseName}\"";
        await command.ExecuteNonQueryAsync();

        using var scope = Services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<CatalogDbContext>().Database.MigrateAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        var builderConnection = new NpgsqlConnectionStringBuilder(
            Environment.GetEnvironmentVariable(AdminConnectionVariable) ?? "Host=localhost")
        {
            Database = _databaseName,
        };

        builder.UseSetting("ConnectionStrings:Catalog", builderConnection.ConnectionString);
        builder.ConfigureServices(ConfigureTestServices);
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        if (string.IsNullOrEmpty(_adminConnection))
        {
            return;
        }

        NpgsqlConnection.ClearAllPools();
        await using var connection = new NpgsqlConnection(_adminConnection);
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = $"DROP DATABASE IF EXISTS \"{_databaseName}\" WITH (FORCE)";
        await command.ExecuteNonQueryAsync();
    }
}
