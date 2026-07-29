using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace StellarAdmin.Shell;

public static class StellarAdminEndpointRouteBuilderExtensions
{
    extension(IEndpointRouteBuilder endpoints)
    {
        public void MapStellarAdmin()
        {
            endpoints.MapStellarAdmin("/stellaradmin");
        }

        public void MapStellarAdmin(PathString stellarAdminPath)
        {
            var routePrefix = stellarAdminPath.ToString().Trim('/');

            endpoints
                .MapAreaControllerRoute(
                    name: "StellarAdmin",
                    areaName: "StellarAdmin",
                    pattern: $"{routePrefix}/{{controller=Home}}/{{action=Index}}/{{id?}}"
                )
                // Conventional routing is order-dependent, and link generation resolves ties by endpoint
                // Order (lowest wins). Since StellarAdmin is a library, the host controls where MapStellarAdmin
                // sits relative to its own default route; without pinning the Order here, a default route
                // registered before this call captures asp-area links and spills "area" into the query string.
                // A negative Order keeps StellarAdmin's routes ahead of normally-registered routes regardless
                // of registration position.
                .Add(builder => ((RouteEndpointBuilder)builder).Order = -1);
        }
    }
}
