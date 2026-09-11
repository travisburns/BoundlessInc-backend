using BoundlessEnterprises.Domain.Companies;
using Xunit;

namespace BoundlessEnterprises.UnitTests.Companies;

public class CompanyTests
{
    [Fact]
    public void Create_NormalizesSlugAndCode_AndStartsComingSoon()
    {
        var company = Company.Create("Firefin", "Firefin", "fire-001", CompanyType.Subsidiary);

        Assert.Equal("Firefin", company.Name);
        Assert.Equal("firefin", company.Slug);
        Assert.Equal("FIRE-001", company.Code);
        Assert.Equal(CompanyStatus.ComingSoon, company.Status);
        Assert.True(company.IsPubliclyVisible);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_Throws_WhenNameMissing(string name)
    {
        Assert.Throws<ArgumentException>(() =>
            Company.Create(name, "slug", "CODE-1", CompanyType.Subsidiary));
    }

    [Fact]
    public void Archive_MakesCompanyNotPubliclyVisible()
    {
        var company = Company.Create("Boundless", "boundless", "BND-001", CompanyType.Subsidiary);
        company.Activate();
        Assert.True(company.IsPubliclyVisible);

        company.Archive();

        Assert.Equal(CompanyStatus.Archived, company.Status);
        Assert.False(company.IsPubliclyVisible);
    }
}
