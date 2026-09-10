using BoundlessEnterprises.Application.Common.Interfaces;
using BoundlessEnterprises.Infrastructure.Common;
using BoundlessEnterprises.Infrastructure.Persistence;
using BoundlessEnterprises.Infrastructure.Persistence.Interceptors;
using BoundlessEnterprises.Infrastructure.Persistence.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BoundlessEnterprises.Infrastructure;

/// <summary>
/// Registers the Infrastructure layer: EF Core against SQL Server, the auditing
/// interceptor, the clock, and the database seeder.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException(
                "Connection string 'Default' was not found.");

        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        services.AddScoped<AuditableEntityInterceptor>();

        services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            options.UseSqlServer(connectionString, sql =>
                sql.MigrationsHistoryTable("__EFMigrationsHistory", "core"));
            options.AddInterceptors(sp.GetRequiredService<AuditableEntityInterceptor>());
        });

        services.AddScoped<IApplicationDbContext>(sp =>
            sp.GetRequiredService<ApplicationDbContext>());

        services.AddScoped<ApplicationDbSeeder>();

        return services;
    }
}
