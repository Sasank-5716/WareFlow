namespace Wareflow.Application.Tenancy;

/// <summary>Payload for creating a company.</summary>
public sealed record CreateTenantRequest(string Name, string Slug);

/// <summary>Payload for updating a company profile.</summary>
public sealed record UpdateTenantProfileRequest(string Name, string Slug);

/// <summary>Payload for updating company regional settings.</summary>
public sealed record UpdateTenantSettingsRequest(
    string TimeZoneId,
    string Locale,
    string DefaultCurrencyCode);

/// <summary>Payload for inviting a user to a company.</summary>
public sealed record InviteTenantUserRequest(string Email);

/// <summary>Represents a company available to the signed-in user.</summary>
public sealed record TenantSummary(
    Guid Id,
    string Name,
    string Slug,
    string Role,
    string MembershipStatus);

/// <summary>Represents the active company context.</summary>
public sealed record ActiveTenantContext(
    Guid Id,
    string Name,
    string Slug,
    string Role,
    string MembershipStatus);

/// <summary>Represents a pending invitation without exposing its token.</summary>
public sealed record TenantInvitationSummary(
    Guid Id,
    Guid TenantId,
    string Email,
    string Status,
    DateTimeOffset ExpiresAt);

/// <summary>Result of creating an invitation.</summary>
public sealed record TenantInvitationCreated(
    TenantInvitationSummary Invitation);

/// <summary>Represents a tenant membership returned to an authorized administrator.</summary>
public sealed record TenantMemberSummary(
    Guid MembershipId,
    string IdentityUserId,
    string Role,
    string Status);

/// <summary>Payload for assigning a tenant-scoped role.</summary>
public sealed record ChangeTenantMemberRoleRequest(string Role);

public interface ITenantManagementService
{
    Task<TenantSummary> CreateTenantAsync(
        CreateTenantRequest request,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<TenantSummary>> ListTenantsAsync(
        CancellationToken cancellationToken);

    Task<ActiveTenantContext?> GetActiveTenantAsync(
        CancellationToken cancellationToken);

    Task<ActiveTenantContext?> SelectTenantAsync(
        Guid tenantId,
        CancellationToken cancellationToken);

    Task<ActiveTenantContext?> UpdateTenantProfileAsync(
        UpdateTenantProfileRequest request,
        CancellationToken cancellationToken);

    Task<TenantSettingsSnapshot?> UpdateTenantSettingsAsync(
        UpdateTenantSettingsRequest request,
        CancellationToken cancellationToken);

    Task<TenantInvitationCreated?> InviteAsync(
        InviteTenantUserRequest request,
        CancellationToken cancellationToken);

    Task<bool> AcceptInvitationAsync(
        string token,
        CancellationToken cancellationToken);

    Task<bool> RejectInvitationAsync(
        string token,
        CancellationToken cancellationToken);

    Task<bool> RevokeInvitationAsync(
        Guid invitationId,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<TenantInvitationSummary>> ListInvitationsAsync(
        CancellationToken cancellationToken);

    Task<IReadOnlyList<TenantMemberSummary>> ListMembersAsync(
        CancellationToken cancellationToken);

    Task<bool> ChangeMemberRoleAsync(
        string identityUserId,
        ChangeTenantMemberRoleRequest request,
        CancellationToken cancellationToken);

    Task<bool> DisableMemberAsync(
        string identityUserId,
        CancellationToken cancellationToken);
}

public interface ITenantContextResolver
{
    Task<bool> TrySelectTenantAsync(
        Guid tenantId,
        CancellationToken cancellationToken);
}
