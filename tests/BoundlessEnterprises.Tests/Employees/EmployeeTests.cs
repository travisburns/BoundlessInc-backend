using BoundlessEnterprises.Domain.Employees;
using Xunit;

namespace BoundlessEnterprises.UnitTests.Employees;

public class EmployeeTests
{
    private static Employee NewEmployee(Guid companyId) =>
        Employee.Create(companyId, "Ada", "Vale", "Ada.Vale@Example.com", "555-0100",
            "Chief of Staff", "Executive", EmploymentType.FullTime, new DateOnly(2026, 1, 15));

    [Fact]
    public void Create_SetsTenant_NormalizesEmail_AndCreatesPrimaryEmployment()
    {
        var companyId = Guid.NewGuid();
        var employee = NewEmployee(companyId);

        Assert.Equal(companyId, employee.CompanyId);
        Assert.Equal("ada.vale@example.com", employee.Email);
        Assert.Equal("Ada Vale", employee.FullName);
        Assert.Equal(EmployeeStatus.Pending, employee.Status);

        var primary = employee.PrimaryEmployment;
        Assert.NotNull(primary);
        Assert.True(primary!.IsPrimary);
        Assert.Equal("Chief of Staff", primary.Title);
        Assert.Equal(companyId, primary.CompanyId);
    }

    [Fact]
    public void Create_Throws_WhenCompanyMissing()
    {
        Assert.Throws<ArgumentException>(() =>
            Employee.Create(Guid.Empty, "A", "B", "a@b.com", null, "T", null,
                EmploymentType.FullTime, new DateOnly(2026, 1, 1)));
    }

    [Fact]
    public void StatusTransitions_Work()
    {
        var employee = NewEmployee(Guid.NewGuid());

        employee.Activate();
        Assert.Equal(EmployeeStatus.Active, employee.Status);

        employee.PlaceOnLeave();
        Assert.Equal(EmployeeStatus.OnLeave, employee.Status);

        employee.Terminate();
        Assert.Equal(EmployeeStatus.Terminated, employee.Status);
    }
}
