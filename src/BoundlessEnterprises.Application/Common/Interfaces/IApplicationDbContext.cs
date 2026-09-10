using BoundlessEnterprises.Domain.Companies;
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

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
