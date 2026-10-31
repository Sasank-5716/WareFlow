using Microsoft.EntityFrameworkCore;
using Wareflow.Application.Tenancy;
using Wareflow.Domain.Auditing;
using Wareflow.Domain.Tenancy;

namespace Wareflow.Infrastructure.Persistence;

public sealed class WareflowDbContext(
    DbContextOptions<WareflowDbContext> options,
    ITenantContext tenantContext)
    : DbContext(options)
{
    public DbSet<Tenant> Tenants => Set<Tenant>();

    public DbSet<TenantMembership> TenantMemberships => Set<TenantMembership>();

    public DbSet<TenantSettings> TenantSettings => Set<TenantSettings>();

    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    public Guid? CurrentTenantId => tenantContext.TenantId;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(WareflowDbContext).Assembly);
        modelBuilder.Entity<TenantMembership>()
            .HasQueryFilter(entity =>
                CurrentTenantId.HasValue &&
                entity.TenantId == CurrentTenantId.Value);
        modelBuilder.Entity<TenantSettings>()
            .HasQueryFilter(entity =>
                CurrentTenantId.HasValue &&
                entity.TenantId == CurrentTenantId.Value);
        modelBuilder.Entity<AuditLog>()
            .HasQueryFilter(entity =>
                CurrentTenantId.HasValue &&
                entity.TenantId == CurrentTenantId.Value);
        base.OnModelCreating(modelBuilder);
    }
}
