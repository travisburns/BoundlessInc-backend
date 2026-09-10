using BoundlessEnterprises.Domain.Companies;
using BoundlessEnterprises.Domain.Employees;
using BoundlessEnterprises.Domain.Identity;
using BoundlessEnterprises.Domain.Onboarding;
using BoundlessEnterprises.Domain.Payments;
using Microsoft.EntityFrameworkCore;

namespace BoundlessEnterprises.Application.Common.Interfaces;

/// <summary>
/// Abstraction over the central holding-company database exposed to the
/// Application layer. Keeps EF Core out of use-case code while still allowing
/// LINQ composition against the aggregate sets.
/// </summary>
public interface IApplicationDbContext
{
    DbSet<Company> Companies { get; }
    DbSet<User> Users { get; }
    DbSet<Role> Roles { get; }
    DbSet<Permission> Permissions { get; }
    DbSet<UserCompany> UserCompanies { get; }
    DbSet<Employee> Employees { get; }
    DbSet<Employment> Employments { get; }
    DbSet<OnboardingTemplate> OnboardingTemplates { get; }
    DbSet<OnboardingProcess> OnboardingProcesses { get; }
    DbSet<Customer> Customers { get; }
    DbSet<Invoice> Invoices { get; }
    DbSet<Payment> Payments { get; }
    DbSet<Subscription> Subscriptions { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
