using Wareflow.Infrastructure.Tenancy;

namespace Wareflow.Api.Tenancy;

public sealed class TenantContextMiddleware(
    RequestDelegate next,
    IHostEnvironment environment)
{
    public async Task InvokeAsync(HttpContext httpContext, TenantContext tenantContext)
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

        await next(httpContext);
    }
}
