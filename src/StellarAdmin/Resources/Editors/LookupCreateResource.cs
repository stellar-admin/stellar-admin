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
        (string Cause, string Fix)? problem =
            type is null
                ? (
                    "its items have no type to find a resource for",
                    "Select the resource with `EnableCreate<TResource>()`."
                )
            : resource is null
                ? (
                    $"no resource is registered for `{type.Name}`",
                    $"Add a resource for `{type.Name}`, or remove `EnableCreate`."
                )
            : !resource.ResolveCanCreate(services)
                ? (
                    $"the `{resource.ControllerName}` resource has no create form",
                    $"Allow create on the `{resource.ControllerName}` resource, or remove `EnableCreate`."
                )
            : null;

        // Other resources aren't known while a resource's options are built, so this is found when the lookup renders
        if (problem is not null)
        {
            services
                .GetRequiredService<ConfigurationProblems>()
                .Report(
                    new(
                        ConfigurationProblemKind.LookupCreateUnavailable,
                        $"The `{editorName}` on `{fieldName}` has no New button because {problem.Value.Cause}.",
                        problem.Value.Fix
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
