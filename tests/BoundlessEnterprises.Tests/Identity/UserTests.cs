using BoundlessEnterprises.Domain.Identity;
using Xunit;

namespace BoundlessEnterprises.UnitTests.Identity;

public class UserTests
{
    [Fact]
    public void Create_NormalizesEmail_AndIsActive()
    {
        var user = User.Create("Admin@Boundless.Enterprises", "hash", "Platform", "Admin");

        Assert.Equal("admin@boundless.enterprises", user.Email);
        Assert.True(user.IsActive);
        Assert.False(user.IsPlatformAdmin);
        Assert.Equal("Platform Admin", user.FullName);
    }

    [Fact]
    public void AddMembership_IsIdempotentPerCompany()
    {
        var user = User.Create("a@b.com", "hash", "A", "B");
        var companyId = Guid.NewGuid();

        var first = user.AddMembership(companyId, isPrimary: true);
        var second = user.AddMembership(companyId);

        Assert.Single(user.Memberships);
        Assert.Same(first, second);
    }

    [Fact]
    public void AssignRole_OnMembership_IsIdempotent()
    {
        var user = User.Create("a@b.com", "hash", "A", "B");
        var membership = user.AddMembership(Guid.NewGuid());
        var roleId = Guid.NewGuid();

        membership.AssignRole(roleId);
        membership.AssignRole(roleId);

        Assert.Single(membership.Roles);
    }

    [Fact]
    public void ResetToken_IsValidOnlyWhenMatchingAndUnexpired()
    {
        var user = User.Create("a@b.com", "hash", "A", "B");
        var now = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        user.SetPasswordResetToken("tokenhash", now.AddMinutes(30));

        Assert.True(user.IsResetTokenValid("tokenhash", now));
        Assert.False(user.IsResetTokenValid("wrong", now));
        Assert.False(user.IsResetTokenValid("tokenhash", now.AddHours(1)));
    }
}
