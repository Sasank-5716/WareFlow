using Wareflow.Domain.Tenancy;

namespace Wareflow.UnitTests;

public sealed class TenantOnboardingTests
{
    [Fact]
    public void Membership_can_be_disabled_and_reenabled()
    {
        var membership = new TenantMembership(
            Guid.NewGuid(),
            "user-1",
            TenantMembershipRole.Member,
            DateTime.UtcNow);

        membership.Disable();
        Assert.Equal(TenantMembershipStatus.Disabled, membership.Status);

        membership.Enable();
        Assert.Equal(TenantMembershipStatus.Active, membership.Status);
    }

    [Fact]
    public void Expired_invitation_cannot_be_accepted()
    {
        var created = DateTime.UtcNow;
        var invitation = new TenantInvitation(
            Guid.NewGuid(),
            "user@example.com",
            "hash",
            created.AddMinutes(1),
            "owner-1",
            created);

        invitation.Expire(created.AddMinutes(2));

        Assert.Equal(TenantInvitationStatus.Expired, invitation.Status);
        Assert.Throws<InvalidOperationException>(
            () => invitation.Accept(created.AddMinutes(2)));
    }

    [Fact]
    public void Accepted_invitation_cannot_be_replayed()
    {
        var created = DateTime.UtcNow;
        var invitation = new TenantInvitation(
            Guid.NewGuid(),
            "user@example.com",
            "hash",
            created.AddDays(1),
            "owner-1",
            created);

        invitation.Accept(created.AddMinutes(1));

        Assert.Throws<InvalidOperationException>(
            () => invitation.Accept(created.AddMinutes(2)));
    }
}
