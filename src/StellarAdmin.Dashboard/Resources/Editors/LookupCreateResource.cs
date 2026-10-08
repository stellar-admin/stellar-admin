using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Dashboard.Infrastructure.Authorization;

namespace StellarAdmin.Dashboard.Resources.Editors;

// The resource registered for a lookup's items creates new items; a user it doesn't authorize gets no New button
internal static class LookupCreateResource
{
    public static async Task<string?> FindControllerAsync(
        IServiceProvider services,
        string editorName,
        string fieldName,
        LookupItems items,
        Type? resourceType
    )
    {
        var type =
            resourceType
            ?? items.ItemType
            ?? throw new InvalidOperationException(
                $"{editorName} on {fieldName} enables create, but its items have no type. Use EnableCreate<TResource>() to select the resource."
            );
        var resource =
            services
                .GetServices<ResourceRegistration>()
                .FirstOrDefault(registration => registration.ResourceType == type)
            ?? throw new InvalidOperationException(
                $"{editorName} on {fieldName} enables create, but no resource is registered for {type.Name}."
            );
        if (!resource.ResolveCanCreate(services))
        {
            throw new InvalidOperationException(
                $"{editorName} on {fieldName} enables create, but the {resource.ControllerName} resource has no create form."
            );
        }

        var httpContext = services.GetRequiredService<IHttpContextAccessor>().HttpContext!;

        return await AuthorizationMetadata.AuthorizeAsync(
            resource.ResolveAuthorizationMetadata(services),
            httpContext
        )
            ? resource.ControllerName
            : null;
    }
}
