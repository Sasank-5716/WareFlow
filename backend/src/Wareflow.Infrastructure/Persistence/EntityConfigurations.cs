using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wareflow.Domain.Auditing;
using Wareflow.Domain.Modules;
using Wareflow.Domain.Tenancy;

namespace Wareflow.Infrastructure.Persistence;

internal sealed class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("tenants");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.Id).ValueGeneratedNever();
        builder.Property(entity => entity.Name).HasMaxLength(200).IsRequired();
        builder.Property(entity => entity.Slug).HasMaxLength(100).IsRequired();
        builder.HasIndex(entity => entity.Slug).IsUnique();
        builder.Property(entity => entity.CreatedAtUtc).IsRequired();
        builder.Property(entity => entity.UpdatedAtUtc).IsRequired();
    }
}

internal sealed class TenantMembershipConfiguration : IEntityTypeConfiguration<TenantMembership>
{
    public void Configure(EntityTypeBuilder<TenantMembership> builder)
    {
        builder.ToTable("tenant_memberships");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.Id).ValueGeneratedNever();
        builder.Property(entity => entity.TenantId).IsRequired();
        builder.Property(entity => entity.IdentityUserId).HasMaxLength(200).IsRequired();
        builder.Property(entity => entity.Role).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(entity => entity.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.HasIndex(entity => new { entity.TenantId, entity.IdentityUserId }).IsUnique();
        builder.HasOne<Tenant>()
            .WithMany()
            .HasForeignKey(entity => entity.TenantId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Property(entity => entity.CreatedAtUtc).IsRequired();
        builder.Property(entity => entity.UpdatedAtUtc).IsRequired();
    }
}

internal sealed class TenantInvitationConfiguration : IEntityTypeConfiguration<TenantInvitation>
{
    public void Configure(EntityTypeBuilder<TenantInvitation> builder)
    {
        builder.ToTable("tenant_invitations");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.Id).ValueGeneratedNever();
        builder.Property(entity => entity.TenantId).IsRequired();
        builder.Property(entity => entity.Email).HasMaxLength(256).IsRequired();
        builder.Property(entity => entity.TokenHash).HasMaxLength(128).IsRequired();
        builder.Property(entity => entity.InvitedByIdentityUserId).HasMaxLength(200).IsRequired();
        builder.Property(entity => entity.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(entity => entity.ExpiresAtUtc).IsRequired();
        builder.HasIndex(entity => new { entity.TenantId, entity.Email, entity.Status });
        builder.HasIndex(entity => entity.TokenHash).IsUnique();
        builder.HasOne<Tenant>()
            .WithMany()
            .HasForeignKey(entity => entity.TenantId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Property(entity => entity.CreatedAtUtc).IsRequired();
        builder.Property(entity => entity.UpdatedAtUtc).IsRequired();
    }
}

internal sealed class TenantSettingsConfiguration : IEntityTypeConfiguration<TenantSettings>
{
    public void Configure(EntityTypeBuilder<TenantSettings> builder)
    {
        builder.ToTable("tenant_settings");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.Id).ValueGeneratedNever();
        builder.Property(entity => entity.TenantId).IsRequired();
        builder.Property(entity => entity.TimeZoneId).HasMaxLength(100).IsRequired();
        builder.Property(entity => entity.Locale).HasMaxLength(20).IsRequired();
        builder.Property(entity => entity.DefaultCurrencyCode).HasMaxLength(3).IsRequired();
        builder.HasIndex(entity => entity.TenantId).IsUnique();
        builder.HasOne<Tenant>()
            .WithMany()
            .HasForeignKey(entity => entity.TenantId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Property(entity => entity.CreatedAtUtc).IsRequired();
        builder.Property(entity => entity.UpdatedAtUtc).IsRequired();
    }
}

internal sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("audit_logs");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.Id).ValueGeneratedNever();
        builder.Property(entity => entity.TenantId).IsRequired();
        builder.Property(entity => entity.Action).HasMaxLength(100).IsRequired();
        builder.Property(entity => entity.EntityType).HasMaxLength(200).IsRequired();
        builder.Property(entity => entity.EntityId).HasMaxLength(200);
        builder.Property(entity => entity.ActorIdentityUserId).HasMaxLength(200);
        builder.Property(entity => entity.CorrelationId).HasMaxLength(100);
        builder.Property(entity => entity.SafeMetadataJson).HasColumnType("jsonb");
        builder.Property(entity => entity.OccurredAtUtc).IsRequired();
        builder.HasIndex(entity => new { entity.TenantId, entity.OccurredAtUtc });
        builder.HasOne<Tenant>()
            .WithMany()
            .HasForeignKey(entity => entity.TenantId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Property(entity => entity.CreatedAtUtc).IsRequired();
        builder.Property(entity => entity.UpdatedAtUtc).IsRequired();
    }

    internal sealed class TenantModuleSettingConfiguration : IEntityTypeConfiguration<TenantModuleSetting>
    {
        public void Configure(EntityTypeBuilder<TenantModuleSetting> builder)
        {
            builder.ToTable("tenant_module_settings");
            builder.HasKey(entity => entity.Id);
            builder.Property(entity => entity.Id).ValueGeneratedNever();
            builder.Property(entity => entity.TenantId).IsRequired();
            builder.Property(entity => entity.ModuleKey).HasMaxLength(100).IsRequired();
            builder.Property(entity => entity.Enabled).IsRequired();
            builder.HasIndex(entity => new { entity.TenantId, entity.ModuleKey }).IsUnique();
            builder.HasOne<Tenant>()
                .WithMany()
                .HasForeignKey(entity => entity.TenantId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.Property(entity => entity.CreatedAtUtc).IsRequired();
            builder.Property(entity => entity.UpdatedAtUtc).IsRequired();
        }
    }

    internal sealed class TenantSubscriptionEntitlementConfiguration : IEntityTypeConfiguration<TenantSubscriptionEntitlement>
    {
        public void Configure(EntityTypeBuilder<TenantSubscriptionEntitlement> builder)
        {
            builder.ToTable("tenant_subscription_entitlements");
            builder.HasKey(entity => entity.Id);
            builder.Property(entity => entity.Id).ValueGeneratedNever();
            builder.Property(entity => entity.TenantId).IsRequired();
            builder.Property(entity => entity.PlanKey).HasMaxLength(100).IsRequired();
            builder.Property(entity => entity.ModuleKey).HasMaxLength(100).IsRequired();
            builder.Property(entity => entity.Included).IsRequired();
            builder.Property(entity => entity.ExpiresAtUtc);
            builder.HasIndex(entity => new { entity.TenantId, entity.ModuleKey }).IsUnique();
            builder.HasOne<Tenant>()
                .WithMany()
                .HasForeignKey(entity => entity.TenantId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.Property(entity => entity.CreatedAtUtc).IsRequired();
            builder.Property(entity => entity.UpdatedAtUtc).IsRequired();
        }
    }
}
