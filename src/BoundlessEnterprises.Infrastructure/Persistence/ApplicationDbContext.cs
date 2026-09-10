using System.Reflection;
using BoundlessEnterprises.Application.Common.Interfaces;
using BoundlessEnterprises.Domain.Common;
using BoundlessEnterprises.Domain.Companies;
using BoundlessEnterprises.Domain.Employees;
using BoundlessEnterprises.Domain.Identity;
using BoundlessEnterprises.Domain.Onboarding;
using BoundlessEnterprises.Domain.Payments;
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
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<UserCompany> UserCompanies => Set<UserCompany>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Employment> Employments => Set<Employment>();
    public DbSet<OnboardingTemplate> OnboardingTemplates => Set<OnboardingTemplate>();
    public DbSet<OnboardingProcess> OnboardingProcesses => Set<OnboardingProcess>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();

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
