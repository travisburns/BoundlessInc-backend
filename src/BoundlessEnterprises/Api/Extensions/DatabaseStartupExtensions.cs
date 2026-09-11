using BoundlessEnterprises.Infrastructure.Persistence;
using BoundlessEnterprises.Infrastructure.Persistence.Seed;
using Microsoft.EntityFrameworkCore;

namespace BoundlessEnterprises.Api.Extensions;

/// <summary>
/// Applies pending EF Core migrations and seeds baseline data at startup.
/// Intended for development and first-run environments.
/// </summary>
public static class DatabaseStartupExtensions
{
    public static async Task InitializeDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<Program>>();

        try
        {
            var db = services.GetRequiredService<ApplicationDbContext>();
            logger.LogInformation("Applying database migrations...");
            await db.Database.MigrateAsync();

            var seeder = services.GetRequiredService<ApplicationDbSeeder>();
            await seeder.SeedAsync();
            logger.LogInformation("Database initialization complete.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Database initialization failed.");
            throw;
        }
    }
}
