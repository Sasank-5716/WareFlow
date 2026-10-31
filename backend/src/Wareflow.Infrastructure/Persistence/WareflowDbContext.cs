using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Wareflow.Application.Tenancy;
using Wareflow.Domain.Auditing;
using Wareflow.Domain.Tenancy;
using Wareflow.Infrastructure.Identity;

namespace Wareflow.Infrastructure.Persistence;

public sealed class WareflowDbContext(
    DbContextOptions<WareflowDbContext> options,
    ITenantContext tenantContext)
    : IdentityDbContext<WareflowIdentityUser>(options)
{
    public DbSet<Tenant> Tenants => Set<Tenant>();

    public DbSet<TenantMembership> TenantMemberships => Set<TenantMembership>();

    public DbSet<TenantSettings> TenantSettings => Set<TenantSettings>();

    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    public DbSet<TenantInvitation> TenantInvitations => Set<TenantInvitation>();

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
        modelBuilder.Entity<TenantInvitation>()
            .HasQueryFilter(entity =>
                CurrentTenantId.HasValue &&
                entity.TenantId == CurrentTenantId.Value);
        base.OnModelCreating(modelBuilder);
    }
}
