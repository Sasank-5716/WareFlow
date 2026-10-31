using Wareflow.Domain.Common;

namespace Wareflow.Domain.Tenancy;

public sealed class TenantInvitation : TenantOwnedEntity
{
    private TenantInvitation()
    {
    }

    public TenantInvitation(
        Guid tenantId,
        string email,
        string tokenHash,
        DateTime expiresAtUtc,
        string invitedByIdentityUserId,
        DateTime createdAtUtc)
        : base(tenantId, createdAtUtc)
    {
        Email = Require(email, nameof(email)).ToLowerInvariant();
        TokenHash = Require(tokenHash, nameof(tokenHash));
        InvitedByIdentityUserId = Require(
            invitedByIdentityUserId,
            nameof(invitedByIdentityUserId));
        ExpiresAtUtc = DateTime.SpecifyKind(expiresAtUtc, DateTimeKind.Utc);
        Status = TenantInvitationStatus.Pending;
    }

    public string Email { get; private set; } = null!;

    public string TokenHash { get; private set; } = null!;

    public string InvitedByIdentityUserId { get; private set; } = null!;

    public DateTime ExpiresAtUtc { get; private set; }

    public DateTime? RespondedAtUtc { get; private set; }

    public TenantInvitationStatus Status { get; private set; }

    public void Accept(DateTime nowUtc)
    {
        EnsurePending(nowUtc);
        Status = TenantInvitationStatus.Accepted;
        RespondedAtUtc = DateTime.SpecifyKind(nowUtc, DateTimeKind.Utc);
    }

    public void Reject(DateTime nowUtc)
    {
        EnsurePending(nowUtc);
        Status = TenantInvitationStatus.Rejected;
        RespondedAtUtc = DateTime.SpecifyKind(nowUtc, DateTimeKind.Utc);
    }

    public void Revoke(DateTime nowUtc)
    {
        EnsurePending(nowUtc);
        Status = TenantInvitationStatus.Revoked;
        RespondedAtUtc = DateTime.SpecifyKind(nowUtc, DateTimeKind.Utc);
    }

    public bool IsExpired(DateTime nowUtc) =>
        Status == TenantInvitationStatus.Pending &&
        DateTime.SpecifyKind(nowUtc, DateTimeKind.Utc) >= ExpiresAtUtc;

    public void Expire(DateTime nowUtc)
    {
        if (IsExpired(nowUtc))
        {
            Status = TenantInvitationStatus.Expired;
            RespondedAtUtc = DateTime.SpecifyKind(nowUtc, DateTimeKind.Utc);
        }
    }

    private void EnsurePending(DateTime nowUtc)
    {
        Expire(nowUtc);
        if (Status != TenantInvitationStatus.Pending)
        {
            throw new InvalidOperationException("The invitation is no longer pending.");
        }
    }

    private static string Require(string value, string parameterName) =>
        string.IsNullOrWhiteSpace(value)
            ? throw new ArgumentException($"{parameterName} is required.", parameterName)
            : value.Trim();
}

public enum TenantInvitationStatus
{
    Pending = 1,
    Accepted = 2,
    Rejected = 3,
    Revoked = 4,
    Expired = 5
}
