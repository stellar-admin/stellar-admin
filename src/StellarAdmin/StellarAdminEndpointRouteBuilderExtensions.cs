using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.StaticAssets.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Infrastructure.Routing;
using StellarAdmin.Resources;

namespace StellarAdmin;

/// <summary>
///     Provides extension methods that map the admin panel's routes.
/// </summary>
public static class StellarAdminEndpointRouteBuilderExtensions
{
    extension(IEndpointRouteBuilder endpoints)
    {
        /// <summary>
        ///     Maps the admin panel under <c>/stellaradmin</c>.
        /// </summary>
        /// <returns>A builder that adds conventions to the admin panel's endpoints.</returns>
        public ControllerActionEndpointConventionBuilder MapStellarAdmin()
        {
            return endpoints.MapStellarAdmin("/stellaradmin");
        }

        /// <summary>
        ///     Maps the admin panel under a path.
        /// </summary>
        /// <param name="stellarAdminPath">The path that the admin panel is served from.</param>
        /// <returns>A builder that adds conventions to the admin panel's endpoints.</returns>
        public ControllerActionEndpointConventionBuilder MapStellarAdmin(
            PathString stellarAdminPath
        )
        {
            var routePrefix = stellarAdminPath.ToString().Trim('/');

            // Only this route references the transformer, so host routes keep their casing.
            var lowercase = LowercaseParameterTransformer.Name;
            var route = endpoints.MapAreaControllerRoute(
                name: "StellarAdmin",
                areaName: "StellarAdmin",
                pattern: $"{routePrefix}/{{controller:{lowercase}=Home}}/{{action:{lowercase}=Index}}/{{id?}}"
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

            // The layout's import map lists the fingerprinted URLs of the client modules, read from
            // the static asset metadata on the page's endpoint. WithStaticAssets() can't add it to
            // conventional controller routes (dotnet/aspnetcore#59003), so it's added here. Skipped
            // when the host or a fixed framework already added it.
            var staticAssets = new Lazy<ResourceAssetCollection>(() =>
                ResolveStaticAssets(endpoints)
            );
            route.Add(builder =>
            {
                if (!builder.Metadata.OfType<ResourceAssetCollection>().Any())
                {
                    builder.Metadata.Add(staticAssets.Value);
                    builder.Metadata.Add(
                        ImportMapDefinition.FromResourceCollection(staticAssets.Value)
                    );
                }
            });

            return route;
        }
    }

    private static void AddAuthorizationMetadata(EndpointBuilder builder, IServiceProvider services)
    {
        if (services.GetService<StellarAdminOptions>() is { } options)
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

    // Mirrors the collection WithStaticAssets() builds: the label is an asset's plain path, and its
    // route the fingerprinted one.
    private static ResourceAssetCollection ResolveStaticAssets(IEndpointRouteBuilder endpoints)
    {
        var assets = new List<ResourceAsset>();
        foreach (
            var descriptor in StaticAssetsEndpointDataSourceHelper.ResolveStaticAssetDescriptors(
                endpoints,
                null
            )
        )
        {
            // Compressed variants share the asset's URL.
            if (descriptor.Selectors.Count > 0)
            {
                continue;
            }

            var properties = descriptor
                .Properties.Where(property => property.Name is "label" or "integrity")
                .Select(property => new ResourceAssetProperty(property.Name, property.Value))
                .ToList();
            assets.Add(
                new ResourceAsset(descriptor.Route, properties.Count > 0 ? properties : null)
            );
        }

        return new ResourceAssetCollection(assets);
    }
}
