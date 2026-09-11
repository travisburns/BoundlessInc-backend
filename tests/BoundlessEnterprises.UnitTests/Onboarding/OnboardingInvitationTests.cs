using BoundlessEnterprises.Domain.Onboarding;

namespace BoundlessEnterprises.UnitTests.Onboarding;

public class OnboardingInvitationTests
{
    private static OnboardingInvitation Build(DateTime expiresAtUtc) =>
        OnboardingInvitation.Create(
            Guid.NewGuid(), Guid.NewGuid(), "New.Hire@Example.com ", " Sam", "Rivera ",
            "Kitchen Team Member", "hash", expiresAtUtc);

    [Fact]
    public void Create_NormalizesEmailAndTrimsNames_AndStartsPending()
    {
        var invite = Build(DateTime.UtcNow.AddDays(14));

        Assert.Equal("new.hire@example.com", invite.Email);
        Assert.Equal("Sam", invite.FirstName);
        Assert.Equal("Rivera", invite.LastName);
        Assert.Equal(OnboardingInvitationStatus.Pending, invite.Status);
    }

    [Fact]
    public void Create_BlankTitle_DefaultsToNewEmployee()
    {
        var invite = OnboardingInvitation.Create(
            Guid.NewGuid(), Guid.NewGuid(), "a@b.com", "A", "B", "  ", "hash",
            DateTime.UtcNow.AddDays(1));

        Assert.Equal("New Employee", invite.Title);
    }

    [Fact]
    public void IsRedeemable_TrueWhilePendingAndNotExpired()
    {
        var now = DateTime.UtcNow;
        var invite = Build(now.AddDays(1));

        Assert.True(invite.IsRedeemable(now));
    }

    [Fact]
    public void IsRedeemable_FalseWhenExpired()
    {
        var now = DateTime.UtcNow;
        var invite = Build(now.AddDays(-1));

        Assert.False(invite.IsRedeemable(now));
    }

    [Fact]
    public void MarkStarted_SetsInProgressAndLinksEmployeeAndProcess()
    {
        var invite = Build(DateTime.UtcNow.AddDays(1));
        var employeeId = Guid.NewGuid();
        var processId = Guid.NewGuid();

        invite.MarkStarted(employeeId, processId);

        Assert.Equal(OnboardingInvitationStatus.InProgress, invite.Status);
        Assert.Equal(employeeId, invite.EmployeeId);
        Assert.Equal(processId, invite.ProcessId);
        Assert.True(invite.IsRedeemable(DateTime.UtcNow)); // resumable while in progress
    }

    [Fact]
    public void Revoke_MakesInviteUnredeemable()
    {
        var invite = Build(DateTime.UtcNow.AddDays(1));

        invite.Revoke();

        Assert.Equal(OnboardingInvitationStatus.Revoked, invite.Status);
        Assert.False(invite.IsRedeemable(DateTime.UtcNow));
    }

    [Fact]
    public void UpdateDetails_IgnoresBlankValues()
    {
        var invite = Build(DateTime.UtcNow.AddDays(1));

        invite.UpdateDetails("Alex", "   ");

        Assert.Equal("Alex", invite.FirstName);
        Assert.Equal("Rivera", invite.LastName);
    }
}
