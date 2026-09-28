using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Dashboard.Resources;

namespace StellarAdmin.Dashboard;

/// <summary>
///     Maps the StellarAdmin Dashboard routes.
/// </summary>
public static class StellarAdminEndpointRouteBuilderExtensions
{
    extension(IEndpointRouteBuilder endpoints)
    {
        /// <summary>
        ///     Maps the Dashboard under <c>/stellaradmin</c>.
        /// </summary>
        /// <returns>
        ///     A builder for adding conventions to the Dashboard endpoints.
        /// </returns>
        public ControllerActionEndpointConventionBuilder MapStellarAdmin()
        {
            return endpoints.MapStellarAdmin("/stellaradmin");
        }

        /// <summary>
        ///     Maps the Dashboard under the given path.
        /// </summary>
        /// <param name="stellarAdminPath">The path the Dashboard is served from.</param>
        /// <returns>
        ///     A builder for adding conventions to the Dashboard endpoints.
        /// </returns>
        public ControllerActionEndpointConventionBuilder MapStellarAdmin(
            PathString stellarAdminPath
        )
        {
            var routePrefix = stellarAdminPath.ToString().Trim('/');

            var route = endpoints.MapAreaControllerRoute(
                name: "StellarAdmin",
                areaName: "StellarAdmin",
                pattern: $"{routePrefix}/{{controller=Home}}/{{action=Index}}/{{id?}}"
            );

            // Conventional routing is order-dependent, and link generation resolves ties by endpoint
            // Order (lowest wins). Since StellarAdmin is a library, the host controls where MapStellarAdmin
            // sits relative to its own default route; without pinning the Order here, a default route
            // registered before this call captures asp-area links and spills "area" into the query string.
            // A negative Order keeps StellarAdmin's routes ahead of normally-registered routes regardless
            // of registration position.
            route.Add(builder => ((RouteEndpointBuilder)builder).Order = -1);

            // Authorization is endpoint metadata, so the host's authorization middleware enforces it
            // before MVC runs. Conventions run when the endpoints are built, after configuration is final.
            route.Add(builder => AddAuthorizationMetadata(builder, endpoints.ServiceProvider));

            return route;
        }
    }

    private static void AddAuthorizationMetadata(EndpointBuilder builder, IServiceProvider services)
    {
        if (services.GetService<StellarAdminDashboardOptions>() is { } options)
        {
            foreach (var metadata in options.AuthorizationMetadata)
            {
                builder.Metadata.Add(metadata);
            }
        }

        var controllerType = builder
            .Metadata.OfType<ControllerActionDescriptor>()
            .FirstOrDefault()
            ?.ControllerTypeInfo.AsType();
        var resource = services
            .GetServices<ResourceRegistration>()
            .FirstOrDefault(registration => registration.ControllerType == controllerType);
        if (resource is null)
        {
            return;
        }

        foreach (var metadata in resource.ResolveAuthorizationMetadata(services))
        {
            builder.Metadata.Add(metadata);
        }
    }
}
