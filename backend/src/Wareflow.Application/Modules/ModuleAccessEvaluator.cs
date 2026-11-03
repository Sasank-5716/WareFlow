namespace Wareflow.Application.Modules;

public static class ModuleAccessEvaluator
{
    public static bool HasAccess(
        IModuleCatalog catalog,
        string moduleKey,
        IReadOnlySet<string> enabledModuleKeys,
        IReadOnlySet<string> entitledModuleKeys,
        IReadOnlySet<string> userPermissions)
    {
        var module = catalog.FindModule(moduleKey);
        return module is not null &&
            module.Availability == ModuleAvailability.Available &&
            enabledModuleKeys.Contains(module.Key) &&
            entitledModuleKeys.Contains(module.Key) &&
            module.RequiredPermissions.IsSubsetOf(userPermissions) &&
            DependenciesSatisfied(
                catalog,
                module,
                enabledModuleKeys,
                entitledModuleKeys);
    }

    public static bool DependenciesSatisfied(
        IModuleCatalog catalog,
        ModuleDefinition module,
        IReadOnlySet<string> enabledModuleKeys,
        IReadOnlySet<string> entitledModuleKeys) =>
        module.Dependencies.All(dependency =>
        {
            var dependencyModule = catalog.FindModule(dependency);
            return dependencyModule is not null &&
                enabledModuleKeys.Contains(dependency) &&
                entitledModuleKeys.Contains(dependency) &&
                DependenciesSatisfied(
                    catalog,
                    dependencyModule,
                    enabledModuleKeys,
                    entitledModuleKeys);
        });
}
