namespace Wareflow.Application.Authorization;

public static class AuthorizationDevelopmentSeed
{
    public static void Validate(IAuthorizationCatalog catalog)
    {
        var roleNames = catalog.Roles
            .Select(role => role.Name)
            .ToHashSet(StringComparer.Ordinal);
        if (roleNames.Count != catalog.Roles.Count ||
            catalog.Roles.Any(role =>
                string.IsNullOrWhiteSpace(role.Name) ||
                role.Permissions.Any(permission => !catalog.IsKnownPermission(permission))))
        {
            throw new InvalidOperationException(
                "The development authorization seed contains invalid roles or permissions.");
        }
    }
}
