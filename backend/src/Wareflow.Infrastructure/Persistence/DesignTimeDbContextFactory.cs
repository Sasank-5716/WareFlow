using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Wareflow.Application.Tenancy;

namespace Wareflow.Infrastructure.Persistence;

public sealed class DesignTimeDbContextFactory
    : IDesignTimeDbContextFactory<WareflowDbContext>
{
    public WareflowDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<WareflowDbContext>();
        optionsBuilder.UseNpgsql(
            "Host=localhost;Port=5432;Database=wareflow;Username=wareflow;Password=change-me-locally");

        return new WareflowDbContext(optionsBuilder.Options, new DesignTimeTenantContext());
    }

    private sealed class DesignTimeTenantContext : ITenantContext
    {
        public Guid? TenantId => null;

        public Guid RequireTenantId() =>
            throw new InvalidOperationException("Tenant context is unavailable at design time.");
    }
}
