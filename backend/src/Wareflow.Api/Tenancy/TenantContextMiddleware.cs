using Wareflow.Infrastructure.Tenancy;
using Wareflow.Application.Tenancy;

namespace Wareflow.Api.Tenancy;

public sealed class TenantContextMiddleware(
    RequestDelegate next,
    IHostEnvironment environment)
{
    public async Task InvokeAsync(
        HttpContext httpContext,
        TenantContext tenantContext,
        ITenantContextResolver resolver)
    {
        if (httpContext.Request.Headers.TryGetValue("X-Test-Tenant-Id", out var value))
        {
            if (!environment.IsDevelopment())
            {
                httpContext.Response.StatusCode = StatusCodes.Status404NotFound;
                return;
            }

            if (!Guid.TryParse(value, out var tenantId))
            {
                httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                return;
            }

            tenantContext.SetTenant(tenantId);
        }
        else if (!IsContextIndependentRoute(httpContext) &&
                 httpContext.Request.Headers.TryGetValue("X-Tenant-Id", out var requestedTenant))
        {
            if (!Guid.TryParse(requestedTenant, out var tenantId) ||
                !await resolver.TrySelectTenantAsync(tenantId, httpContext.RequestAborted))
            {
                httpContext.Response.StatusCode = StatusCodes.Status404NotFound;
                return;
            }
        }

        await next(httpContext);
    }

    private static bool IsContextIndependentRoute(HttpContext context)
    {
        var path = context.Request.Path.Value ?? string.Empty;
        return path.Equals("/api/tenants", StringComparison.OrdinalIgnoreCase) ||
               path.Equals("/api/tenants/invitations/accept", StringComparison.OrdinalIgnoreCase) ||
               path.Equals("/api/tenants/invitations/reject", StringComparison.OrdinalIgnoreCase) ||
               (path.StartsWith("/api/tenants/", StringComparison.OrdinalIgnoreCase) &&
                path.EndsWith("/select", StringComparison.OrdinalIgnoreCase));
    }
}
