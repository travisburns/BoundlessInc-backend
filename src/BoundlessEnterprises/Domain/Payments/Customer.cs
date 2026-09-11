using BoundlessEnterprises.Domain.Common;

namespace BoundlessEnterprises.Domain.Payments;

/// <summary>
/// A billing customer of a specific company. Customers, like every billing
/// record, carry a CompanyId so payments route to the correct company ledger.
/// </summary>
public class Customer : AuditableEntity
{
    private Customer() { }

    private Customer(Guid companyId, string name, string email)
    {
        CompanyId = companyId;
        Name = name;
        Email = email;
    }

    public Guid CompanyId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;

    /// <summary>Payment-gateway customer id (e.g. Stripe cus_…).</summary>
    public string? ExternalId { get; private set; }

    public static Customer Create(Guid companyId, string name, string email)
    {
        if (companyId == Guid.Empty)
            throw new ArgumentException("CompanyId is required.", nameof(companyId));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Customer name is required.", nameof(name));
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Customer email is required.", nameof(email));

        return new Customer(companyId, name.Trim(), email.Trim().ToLowerInvariant());
    }

    public void LinkExternal(string externalId) => ExternalId = externalId;
}
