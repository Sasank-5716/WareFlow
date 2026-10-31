using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Wareflow.Application.Authentication;
using Wareflow.Application.Tenancy;
using Wareflow.Domain.Tenancy;
using Wareflow.Infrastructure.Identity;
using Wareflow.Infrastructure.Persistence;

namespace Wareflow.Infrastructure.Tenancy;

public sealed class TenantManagementService(
    WareflowDbContext dbContext,
    TenantContext tenantContext,
    IHttpContextAccessor httpContextAccessor,
    TimeProvider timeProvider,
    ILogger<TenantManagementService> logger,
    IEmailSender emailSender,
    IOptions<IdentityEmailOptions> emailOptions) : ITenantManagementService, ITenantContextResolver
{
    public async Task<TenantSummary> CreateTenantAsync(
        CreateTenantRequest request,
        CancellationToken cancellationToken)
    {
        var userId = RequireUserId();
        var now = timeProvider.GetUtcNow().UtcDateTime;
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        if (await dbContext.Tenants.AnyAsync(
                tenant => tenant.Slug == request.Slug.Trim().ToLowerInvariant(),
                cancellationToken))
        {
            throw new InvalidOperationException("The company slug is already in use.");
        }

        var tenant = new Tenant(request.Name, request.Slug, now);
        dbContext.Tenants.Add(tenant);
        await dbContext.SaveChangesAsync(cancellationToken);

        tenantContext.SetTenant(tenant.Id);
        await SetDatabaseTenantAsync(tenant.Id, cancellationToken);

        var membership = new TenantMembership(
            tenant.Id,
            userId,
            TenantMembershipRole.Owner,
            now);
        var settings = new TenantSettings(
            tenant.Id,
            "UTC",
            "en-US",
            "USD",
            now);
        dbContext.TenantMemberships.Add(membership);
        dbContext.TenantSettings.Add(settings);
        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return ToSummary(tenant, membership);
    }

    public async Task<IReadOnlyList<TenantSummary>> ListTenantsAsync(
        CancellationToken cancellationToken)
    {
        var userId = RequireUserId();
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        await SetDatabaseUserAsync(userId, cancellationToken);

        var memberships = await dbContext.TenantMemberships
            .IgnoreQueryFilters()
            .Where(membership =>
                membership.IdentityUserId == userId &&
                membership.Status == TenantMembershipStatus.Active)
            .Join(
                dbContext.Tenants,
                membership => membership.TenantId,
                tenant => tenant.Id,
                (membership, tenant) => new { membership, tenant })
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);
        return memberships
            .Select(item => ToSummary(item.tenant, item.membership))
            .ToArray();
    }

    public async Task<ActiveTenantContext?> GetActiveTenantAsync(
        CancellationToken cancellationToken)
    {
        if (!tenantContext.TenantId.HasValue)
        {
            return null;
        }

        var userId = RequireUserId();
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        await SetDatabaseTenantAsync(tenantContext.TenantId.Value, cancellationToken);
        var membership = await dbContext.TenantMemberships
            .AsNoTracking()
            .SingleOrDefaultAsync(
                item => item.IdentityUserId == userId &&
                        item.Status == TenantMembershipStatus.Active,
                cancellationToken);
        var tenant = await dbContext.Tenants
            .AsNoTracking()
            .SingleOrDefaultAsync(
                item => item.Id == tenantContext.TenantId.Value,
                cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return tenant is null || membership is null
            ? null
            : ToActiveContext(tenant, membership);
    }

    public async Task<ActiveTenantContext?> SelectTenantAsync(
        Guid tenantId,
        CancellationToken cancellationToken)
    {
        return await TrySelectTenantAsync(tenantId, cancellationToken)
            ? await GetActiveTenantAsync(cancellationToken)
            : null;
    }

    public async Task<bool> TrySelectTenantAsync(
        Guid tenantId,
        CancellationToken cancellationToken)
    {
        if (tenantId == Guid.Empty ||
            httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated != true)
        {
            return false;
        }

        var userId = RequireUserId();
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        await SetDatabaseTenantAsync(tenantId, cancellationToken);
        await SetDatabaseUserAsync(userId, cancellationToken);
        var membership = await dbContext.TenantMemberships
            .IgnoreQueryFilters()
            .AsNoTracking()
            .SingleOrDefaultAsync(
                item => item.TenantId == tenantId &&
                        item.IdentityUserId == userId &&
                        item.Status == TenantMembershipStatus.Active,
                cancellationToken);

        if (membership is null)
        {
            await transaction.RollbackAsync(cancellationToken);
            return false;
        }

        await transaction.CommitAsync(cancellationToken);
        tenantContext.SetTenant(tenantId);
        return true;
    }

    public async Task<ActiveTenantContext?> UpdateTenantProfileAsync(
        UpdateTenantProfileRequest request,
        CancellationToken cancellationToken)
    {
        var tenant = await GetActiveTenantForWriteAsync(
            TenantMembershipRole.Owner,
            cancellationToken);
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        await SetDatabaseTenantAsync(tenant.Id, cancellationToken);
        tenant.UpdateProfile(request.Name, request.Slug, timeProvider.GetUtcNow().UtcDateTime);
        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return await GetActiveTenantAsync(cancellationToken);
    }

    public async Task<TenantSettingsSnapshot?> UpdateTenantSettingsAsync(
        UpdateTenantSettingsRequest request,
        CancellationToken cancellationToken)
    {
        await GetActiveTenantForWriteAsync(TenantMembershipRole.Owner, cancellationToken);
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        await SetDatabaseTenantAsync(tenantContext.RequireTenantId(), cancellationToken);
        var settings = await dbContext.TenantSettings
            .SingleOrDefaultAsync(cancellationToken);
        if (settings is null)
        {
            await transaction.RollbackAsync(cancellationToken);
            return null;
        }

        settings.Update(
            request.TimeZoneId,
            request.Locale,
            request.DefaultCurrencyCode,
            timeProvider.GetUtcNow().UtcDateTime);
        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return new TenantSettingsSnapshot(
            settings.TenantId,
            settings.TimeZoneId,
            settings.Locale,
            settings.DefaultCurrencyCode);
    }

    public async Task<TenantInvitationCreated?> InviteAsync(
        InviteTenantUserRequest request,
        CancellationToken cancellationToken)
    {
        await GetActiveTenantForWriteAsync(TenantMembershipRole.Owner, cancellationToken);
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        await SetDatabaseTenantAsync(tenantContext.RequireTenantId(), cancellationToken);
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var rawToken = $"{tenantContext.RequireTenantId():N}.{CreateToken()}";
        var invitation = new TenantInvitation(
            tenantContext.RequireTenantId(),
            request.Email,
            HashToken(rawToken),
            now.AddDays(7),
            RequireUserId(),
            now);
        var invitationUrl =
            $"{emailOptions.Value.PublicOrigin.TrimEnd('/')}/invitations/accept?token={Uri.EscapeDataString(rawToken)}";
        try
        {
            await emailSender.SendAsync(
                invitation.Email,
                "You are invited to WAREFLOW",
                $"<p>You have been invited to join a WAREFLOW company. " +
                $"<a href=\"{HtmlEncoder.Default.Encode(invitationUrl)}\">Accept the invitation</a>.</p>",
                cancellationToken);
        }
        catch
        {
            logger.LogWarning(
                "Tenant invitation email delivery failed for tenant {TenantId}",
                invitation.TenantId);
            await transaction.RollbackAsync(cancellationToken);
            return null;
        }

        dbContext.TenantInvitations.Add(invitation);
        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        logger.LogInformation(
            "Tenant invitation created for tenant {TenantId} and email {Email}",
            invitation.TenantId,
            invitation.Email);

        return new TenantInvitationCreated(ToInvitationSummary(invitation));
    }

    public Task<bool> AcceptInvitationAsync(
        string token,
        CancellationToken cancellationToken) =>
        RespondToInvitationAsync(token, accept: true, cancellationToken);

    public Task<bool> RejectInvitationAsync(
        string token,
        CancellationToken cancellationToken) =>
        RespondToInvitationAsync(token, accept: false, cancellationToken);

    public async Task<bool> RevokeInvitationAsync(
        Guid invitationId,
        CancellationToken cancellationToken)
    {
        await GetActiveTenantForWriteAsync(TenantMembershipRole.Owner, cancellationToken);
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        await SetDatabaseTenantAsync(tenantContext.RequireTenantId(), cancellationToken);
        var invitation = await dbContext.TenantInvitations
            .SingleOrDefaultAsync(item => item.Id == invitationId, cancellationToken);
        if (invitation is null)
        {
            await transaction.RollbackAsync(cancellationToken);
            return false;
        }

        var now = timeProvider.GetUtcNow().UtcDateTime;
        invitation.Expire(now);
        if (invitation.Status != TenantInvitationStatus.Pending)
        {
            await transaction.RollbackAsync(cancellationToken);
            return false;
        }

        invitation.Revoke(now);
        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return true;
    }

    public async Task<IReadOnlyList<TenantInvitationSummary>> ListInvitationsAsync(
        CancellationToken cancellationToken)
    {
        await GetActiveTenantForWriteAsync(TenantMembershipRole.Owner, cancellationToken);
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        await SetDatabaseTenantAsync(tenantContext.RequireTenantId(), cancellationToken);
        var invitations = await dbContext.TenantInvitations
            .OrderByDescending(item => item.CreatedAtUtc)
            .ToListAsync(cancellationToken);
        var now = timeProvider.GetUtcNow().UtcDateTime;
        foreach (var invitation in invitations)
        {
            invitation.Expire(now);
        }
        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return invitations.Select(ToInvitationSummary).ToArray();
    }

    private async Task<bool> RespondToInvitationAsync(
        string token,
        bool accept,
        CancellationToken cancellationToken)
    {
        var userId = RequireUserId();
        var userEmail = httpContextAccessor.HttpContext?.User
            .FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
        if (string.IsNullOrWhiteSpace(userEmail))
        {
            return false;
        }

        if (!TryReadTenantId(token, out var tenantId))
        {
            return false;
        }

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        await SetDatabaseTenantAsync(tenantId, cancellationToken);
        await SetDatabaseUserAsync(userId, cancellationToken);
        var invitation = await dbContext.TenantInvitations
            .SingleOrDefaultAsync(
                item => item.TokenHash == HashToken(token) &&
                        item.Email == userEmail.Trim().ToLowerInvariant(),
                cancellationToken);
        if (invitation is null)
        {
            await transaction.RollbackAsync(cancellationToken);
            return false;
        }

        var now = timeProvider.GetUtcNow().UtcDateTime;
        try
        {
            if (accept)
            {
                invitation.Accept(now);
                var membership = await dbContext.TenantMemberships
                    .IgnoreQueryFilters()
                    .SingleOrDefaultAsync(
                        item => item.TenantId == tenantId && item.IdentityUserId == userId,
                        cancellationToken);
                if (membership is null)
                {
                    dbContext.TenantMemberships.Add(new TenantMembership(
                        tenantId,
                        userId,
                        TenantMembershipRole.Member,
                        now));
                }
                else
                {
                    membership.Enable();
                }
            }
            else
            {
                invitation.Reject(now);
            }
        }
        catch (InvalidOperationException)
        {
            await transaction.RollbackAsync(cancellationToken);
            return false;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        tenantContext.SetTenant(tenantId);
        return true;
    }

    private async Task<Tenant> GetActiveTenantForWriteAsync(
        TenantMembershipRole requiredRole,
        CancellationToken cancellationToken)
    {
        var userId = RequireUserId();
        var tenantId = tenantContext.RequireTenantId();
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        await SetDatabaseTenantAsync(tenantId, cancellationToken);
        await SetDatabaseUserAsync(userId, cancellationToken);
        var membership = await dbContext.TenantMemberships
            .IgnoreQueryFilters()
            .SingleOrDefaultAsync(
                item => item.TenantId == tenantId &&
                        item.IdentityUserId == userId &&
                        item.Status == TenantMembershipStatus.Active,
                cancellationToken);
        if (membership is null || membership.Role > requiredRole)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw new UnauthorizedAccessException("The active tenant membership is not authorized.");
        }

        var tenant = await dbContext.Tenants.SingleAsync(
            tenant => tenant.Id == tenantId,
            cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return tenant;
    }

    private async Task SetDatabaseTenantAsync(
        Guid tenantId,
        CancellationToken cancellationToken) =>
        await dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"SELECT set_config('app.tenant_id', {tenantId:N}, true)",
            cancellationToken);

    private async Task SetDatabaseUserAsync(
        string userId,
        CancellationToken cancellationToken) =>
        await dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"SELECT set_config('app.identity_user_id', {userId}, true)",
            cancellationToken);

    private string RequireUserId() =>
        httpContextAccessor.HttpContext?.User
            .FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
        ?? throw new UnauthorizedAccessException("Authentication is required.");

    private static TenantSummary ToSummary(Tenant tenant, TenantMembership membership) =>
        new(tenant.Id, tenant.Name, tenant.Slug, membership.Role.ToString(), membership.Status.ToString());

    private static ActiveTenantContext ToActiveContext(
        Tenant tenant,
        TenantMembership membership) =>
        new(tenant.Id, tenant.Name, tenant.Slug, membership.Role.ToString(), membership.Status.ToString());

    private static TenantInvitationSummary ToInvitationSummary(TenantInvitation invitation) =>
        new(
            invitation.Id,
            invitation.TenantId,
            invitation.Email,
            invitation.Status.ToString(),
            new DateTimeOffset(invitation.ExpiresAtUtc, TimeSpan.Zero));

    private static string CreateToken() =>
        Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
            .TrimEnd('=').Replace('+', '-').Replace('/', '_');

    private static string HashToken(string token) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));

    private static bool TryReadTenantId(string token, out Guid tenantId)
    {
        tenantId = Guid.Empty;
        var separator = token.IndexOf('.');
        return separator > 0 &&
               Guid.TryParseExact(token[..separator], "N", out tenantId);
    }
}
