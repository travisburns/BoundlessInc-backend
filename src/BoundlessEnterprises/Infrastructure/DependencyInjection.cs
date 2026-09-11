using BoundlessEnterprises.Application.Common.Interfaces;
using BoundlessEnterprises.Infrastructure.Common;
using BoundlessEnterprises.Infrastructure.Email;
using BoundlessEnterprises.Infrastructure.Identity;
using BoundlessEnterprises.Infrastructure.Payments;
using BoundlessEnterprises.Infrastructure.Payments.Stripe;
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

        // Identity & auth
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IEmailSender, LoggingEmailSender>();
        services.AddSingleton<IFileStorage, Storage.LocalFileStorage>();

        // Payments: use real Stripe when a secret key is configured, else the
        // stub gateway so the platform runs end-to-end without credentials.
        services.Configure<StripeOptions>(configuration.GetSection(StripeOptions.SectionName));
        var stripe = configuration.GetSection(StripeOptions.SectionName).Get<StripeOptions>();
        if (stripe?.IsConfigured == true)
        {
            services.AddScoped<IPaymentGateway, StripePaymentGateway>();
            services.AddScoped<IPaymentWebhookHandler, StripeWebhookHandler>();
        }
        else
        {
            services.AddScoped<IPaymentGateway, StubPaymentGateway>();
            services.AddScoped<IPaymentWebhookHandler, StubPaymentWebhookHandler>();
        }

        return services;
    }
}
