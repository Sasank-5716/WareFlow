using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Wareflow.Application.Auditing;
using Wareflow.Application.Tenancy;
using Wareflow.Domain.Auditing;
using Wareflow.Infrastructure.Persistence;

namespace Wareflow.Infrastructure.Auditing;

public sealed class AuditLogWriter(
    WareflowDbContext dbContext,
    ITenantContext tenantContext,
    IHttpContextAccessor httpContextAccessor,
    TimeProvider timeProvider) : IAuditLogWriter
{
    public async Task WriteAsync(
        string action,
        string entityType,
        string? entityId,
        string? safeMetadataJson,
        CancellationToken cancellationToken)
    {
        var tenantId = tenantContext.RequireTenantId();
        var actorId = httpContextAccessor.HttpContext?.User
            .FindFirstValue(ClaimTypes.NameIdentifier);
        var correlationId = httpContextAccessor.HttpContext?.TraceIdentifier;
        dbContext.AuditLogs.Add(new AuditLog(
            tenantId,
            action,
            entityType,
            entityId,
            actorId,
            correlationId,
            timeProvider.GetUtcNow().UtcDateTime,
            safeMetadataJson));
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
