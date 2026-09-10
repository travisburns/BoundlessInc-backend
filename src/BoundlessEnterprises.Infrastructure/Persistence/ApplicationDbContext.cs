using System.Reflection;
using BoundlessEnterprises.Application.Common.Interfaces;
using BoundlessEnterprises.Domain.Common;
using BoundlessEnterprises.Domain.Companies;
using Microsoft.EntityFrameworkCore;

namespace BoundlessEnterprises.Infrastructure.Persistence;

/// <summary>
/// The central holding-company database context. Uses SQL Server schemas
/// (core, identity, hr, onboarding, billing, documents, integration,
/// intelligence, audit) to keep capability boundaries visible in SSMS.
/// </summary>
public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Company> Companies => Set<Company>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Default schema for anything not explicitly mapped elsewhere.
        modelBuilder.HasDefaultSchema("core");

        // Domain events are dispatched in-process, never persisted.
        modelBuilder.Ignore<DomainEvent>();

        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }
}
