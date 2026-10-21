using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Diagnostics;
using StellarAdmin.Infrastructure.Authorization;

namespace StellarAdmin.Resources.Editors;

// The resource registered for a lookup's items creates new items; a user it doesn't authorize gets no New button, and
// a lookup whose create has no resource to use reports the problem and gets none either
internal static class LookupCreateResource
{
    public static async Task<string?> FindControllerAsync(
        IServiceProvider services,
        string editorName,
        string fieldName,
        LookupItems items,
        Type? resourceType,
        ConfigurationSource? source
    )
    {
        var httpContext = services.GetRequiredService<IHttpContextAccessor>().HttpContext!;
        var type = resourceType ?? items.ItemType;
        var resource = type is null
            ? null
            : services
                .GetServices<ResourceRegistration>()
                .FirstOrDefault(registration => registration.ResourceType == type);
        var problem =
            type is null
                ? $"{editorName} on {fieldName} enables create, but its items have no type. Use EnableCreate<TResource>() to select the resource."
            : resource is null
                ? $"{editorName} on {fieldName} enables create, but no resource is registered for {type.Name}."
            : !resource.ResolveCanCreate(services)
                ? $"{editorName} on {fieldName} enables create, but the {resource.ControllerName} resource has no create form."
            : null;

        // Other resources aren't known while a resource's options are built, so this is found when the lookup renders
        if (problem is not null)
        {
            services
                .GetRequiredService<ConfigurationProblems>()
                .Report(
                    new(
                        ConfigurationProblemKind.LookupCreateUnavailable,
                        problem,
                        "The lookup has no New button."
                    )
                    {
                        Source = source,
                    },
                    httpContext.GetRouteValue("controller") as string
                );

            return null;
        }

        return await AuthorizationMetadata.AuthorizeAsync(
            resource!.ResolveAuthorizationMetadata(services),
            httpContext
        )
            ? resource.ControllerName
            : null;
    }
}
