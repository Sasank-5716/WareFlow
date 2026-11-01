using Wareflow.Application.Authorization;
using Wareflow.Domain.Tenancy;

namespace Wareflow.UnitTests;

public sealed class PermissionCatalogTests
{
    [Fact]
    public void Every_initial_role_has_a_safe_permission_profile()
    {
        var catalog = new AuthorizationCatalog();

        var initialRoles = catalog.Roles
            .Where(role => role.Name != "Member")
            .ToArray();

        Assert.Equal(8, initialRoles.Length);
        Assert.All(initialRoles, role =>
        {
            Assert.NotEmpty(role.Name);
            Assert.NotNull(role.Permissions);
            Assert.True(role.Permissions.All(catalog.IsKnownPermission));
        });
    }

    [Fact]
    public void Owner_can_manage_billing_but_read_only_user_cannot()
    {
        var catalog = new AuthorizationCatalog();
        var owner = catalog.Roles.Single(role => role.Name == nameof(TenantMembershipRole.Owner));
        var readOnly = catalog.Roles.Single(role => role.Name == nameof(TenantMembershipRole.ReadOnlyUser));

        Assert.Contains(Permissions.BillingManage, owner.Permissions);
        Assert.DoesNotContain(Permissions.BillingManage, readOnly.Permissions);
    }

    [Fact]
    public void Only_owner_can_delegate_owner_role()
    {
        var catalog = new AuthorizationCatalog();
        var administrator = catalog.Roles.Single(role => role.Name == nameof(TenantMembershipRole.CompanyAdministrator));

        Assert.True(administrator.CanAssignRoles);
        Assert.DoesNotContain(
            nameof(TenantMembershipRole.Owner),
            catalog.Roles.Where(role => role.CanAssignRoles && role.Name != nameof(TenantMembershipRole.Owner))
                .Select(role => role.Name));
    }
}
