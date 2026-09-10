using BoundlessEnterprises.Domain.Common;

namespace BoundlessEnterprises.Domain.Employees;

/// <summary>
/// An employee record owned by a company. Tenancy is explicit: every employee
/// carries the <see cref="CompanyId"/> (BusinessId) of the company that employs
/// them. An optional <see cref="UserId"/> links to a platform account when the
/// employee also signs in to the portal.
/// </summary>
public class Employee : AuditableEntity
{
    private readonly List<Employment> _employments = new();

    private Employee() { }

    private Employee(
        Guid companyId,
        string firstName,
        string lastName,
        string email,
        string? phone)
    {
        CompanyId = companyId;
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        Phone = phone;
        Status = EmployeeStatus.Pending;
    }

    /// <summary>The employing company (BusinessId).</summary>
    public Guid CompanyId { get; private set; }

    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string? Phone { get; private set; }

    public EmployeeStatus Status { get; private set; }

    /// <summary>Optional link to the person's platform account.</summary>
    public Guid? UserId { get; private set; }

    public IReadOnlyCollection<Employment> Employments => _employments.AsReadOnly();

    public string FullName => $"{FirstName} {LastName}".Trim();

    public Employment? PrimaryEmployment =>
        _employments.FirstOrDefault(e => e.IsPrimary) ?? _employments.FirstOrDefault();

    /// <summary>
    /// Creates an employee at a company with an initial primary employment.
    /// </summary>
    public static Employee Create(
        Guid companyId,
        string firstName,
        string lastName,
        string email,
        string? phone,
        string title,
        string? department,
        EmploymentType type,
        DateOnly startDate)
    {
        if (companyId == Guid.Empty)
            throw new ArgumentException("CompanyId is required.", nameof(companyId));
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name is required.", nameof(firstName));
        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name is required.", nameof(lastName));
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email is required.", nameof(email));

        var employee = new Employee(companyId, firstName.Trim(), lastName.Trim(),
            email.Trim().ToLowerInvariant(), phone);

        employee._employments.Add(
            Employment.Create(employee.Id, companyId, title, department, type, startDate, isPrimary: true));

        return employee;
    }

    public void UpdateContact(string firstName, string lastName, string email, string? phone)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name is required.", nameof(firstName));
        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name is required.", nameof(lastName));
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email is required.", nameof(email));

        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        Email = email.Trim().ToLowerInvariant();
        Phone = phone;
    }

    public void LinkUser(Guid userId) => UserId = userId;

    public void Activate() => Status = EmployeeStatus.Active;
    public void PlaceOnLeave() => Status = EmployeeStatus.OnLeave;
    public void Terminate() => Status = EmployeeStatus.Terminated;
}
