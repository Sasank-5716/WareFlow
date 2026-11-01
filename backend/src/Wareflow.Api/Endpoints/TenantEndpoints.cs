using System.ComponentModel.DataAnnotations;
using Wareflow.Api.Authorization;
using Wareflow.Application.Authorization;
using Wareflow.Application.Tenancy;

namespace Wareflow.Api.Endpoints;

public static class TenantEndpoints
{
    public static IEndpointRouteBuilder MapTenantEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/tenants")
            .RequireAuthorization()
            .WithTags("Tenants");

        group.MapPost("/", async (
            CreateTenantRequest request,
            ITenantManagementService service,
            CancellationToken cancellationToken) =>
        {
            var tenant = await service.CreateTenantAsync(request, cancellationToken);
            return Results.Created($"/api/tenants/{tenant.Id}", tenant);
        })
        .WithSummary("Create a company")
        .WithDescription("Creates a company and assigns the authenticated user as its owner.")
        .Produces<TenantSummary>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapGet("/", async (
            ITenantManagementService service,
            CancellationToken cancellationToken) =>
            Results.Ok(await service.ListTenantsAsync(cancellationToken)))
            .WithSummary("List the signed-in user's companies")
            .Produces<IReadOnlyList<TenantSummary>>();

        group.MapGet("/active", async (
            ITenantManagementService service,
            CancellationToken cancellationToken) =>
        {
            var active = await service.GetActiveTenantAsync(cancellationToken);
            return active is null ? Results.NotFound() : Results.Ok(active);
        })
        .WithSummary("Get the active company context")
        .Produces<ActiveTenantContext>()
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/{tenantId:guid}/select", async (
            Guid tenantId,
            ITenantManagementService service,
            CancellationToken cancellationToken) =>
        {
            var active = await service.SelectTenantAsync(tenantId, cancellationToken);
            return active is null ? Results.NotFound() : Results.Ok(active);
        })
        .WithSummary("Select an active company")
        .WithDescription("The returned tenant ID must be sent as X-Tenant-Id on subsequent requests.")
        .Produces<ActiveTenantContext>()
        .Produces(StatusCodes.Status404NotFound);

        group.MapPut("/active/profile", async (
            UpdateTenantProfileRequest request,
            ITenantManagementService service,
            CancellationToken cancellationToken) =>
            Results.Ok(await service.UpdateTenantProfileAsync(request, cancellationToken)))
            .RequirePermission(Permissions.SettingsManage)
            .WithSummary("Update the active company profile")
            .Produces<ActiveTenantContext>();

        group.MapPut("/active/settings", async (
            UpdateTenantSettingsRequest request,
            ITenantManagementService service,
            CancellationToken cancellationToken) =>
        {
            var settings = await service.UpdateTenantSettingsAsync(request, cancellationToken);
            return settings is null ? Results.NotFound() : Results.Ok(settings);
        })
        .RequirePermission(Permissions.SettingsManage)
        .WithSummary("Update the active company settings")
        .Produces<TenantSettingsSnapshot>()
        .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/active/invitations", async (
            ITenantManagementService service,
            CancellationToken cancellationToken) =>
            Results.Ok(await service.ListInvitationsAsync(cancellationToken)))
            .RequirePermission(Permissions.UsersView)
            .WithSummary("List active company invitations")
            .Produces<IReadOnlyList<TenantInvitationSummary>>();

        group.MapPost("/active/invitations", async (
            InviteTenantUserRequest request,
            ITenantManagementService service,
            CancellationToken cancellationToken) =>
        {
            var invitation = await service.InviteAsync(request, cancellationToken);
            return invitation is null
                ? Results.NotFound()
                : Results.Created($"/api/tenants/active/invitations/{invitation.Invitation.Id}", invitation);
        })
        .RequirePermission(Permissions.UsersInvite)
        .WithSummary("Invite a user to the active company")
        .Produces<TenantInvitationCreated>(StatusCodes.Status201Created);

        group.MapPost("/active/invitations/{invitationId:guid}/revoke", async (
            Guid invitationId,
            ITenantManagementService service,
            CancellationToken cancellationToken) =>
            await service.RevokeInvitationAsync(invitationId, cancellationToken)
                ? Results.NoContent()
                : Results.NotFound())
            .RequirePermission(Permissions.UsersManage)
            .WithSummary("Revoke a company invitation")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/active/members", async (
            ITenantManagementService service,
            CancellationToken cancellationToken) =>
            Results.Ok(await service.ListMembersAsync(cancellationToken)))
            .RequirePermission(Permissions.UsersView)
            .WithSummary("List active company members")
            .Produces<IReadOnlyList<TenantMemberSummary>>();

        group.MapPut("/active/members/{identityUserId}/role", async (
            string identityUserId,
            ChangeTenantMemberRoleRequest request,
            ITenantManagementService service,
            CancellationToken cancellationToken) =>
            await service.ChangeMemberRoleAsync(identityUserId, request, cancellationToken)
                ? Results.NoContent()
                : Results.NotFound())
            .RequirePermission(Permissions.UsersManage)
            .WithSummary("Assign a tenant-scoped role")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/active/members/{identityUserId}/disable", async (
            string identityUserId,
            ITenantManagementService service,
            CancellationToken cancellationToken) =>
            await service.DisableMemberAsync(identityUserId, cancellationToken)
                ? Results.NoContent()
                : Results.NotFound())
            .RequirePermission(Permissions.UsersManage)
            .WithSummary("Disable a tenant membership")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/invitations/accept", async (
            InvitationTokenRequest request,
            ITenantManagementService service,
            CancellationToken cancellationToken) =>
            await service.AcceptInvitationAsync(request.Token, cancellationToken)
                ? Results.NoContent()
                : Results.NotFound())
            .WithSummary("Accept an invitation")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/invitations/reject", async (
            InvitationTokenRequest request,
            ITenantManagementService service,
            CancellationToken cancellationToken) =>
            await service.RejectInvitationAsync(request.Token, cancellationToken)
                ? Results.NoContent()
                : Results.NotFound())
            .WithSummary("Reject an invitation")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        return endpoints;
    }
}

/// <summary>Payload containing a single invitation token.</summary>
public sealed record InvitationTokenRequest
{
    /// <summary>Single-use invitation token received by the invitee.</summary>
    [Required]
    public required string Token { get; init; }
}
