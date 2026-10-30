using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace StellarAdmin.Infrastructure.Authorization;

// A refused htmx request from the admin panel, such as a row detail loading after the session ended, would
// otherwise follow the challenge's redirect and swap the sign-in page into the page. Instead htmx reloads
// the page, so the app's own challenge or access denied response handles the page. Boosted requests are
// navigations and keep the app's response, as does every request outside the admin panel.
internal sealed class HtmxAuthorizationResultHandler(
    [FromKeyedServices(typeof(HtmxAuthorizationResultHandler))]
        IAuthorizationMiddlewareResultHandler inner
) : IAuthorizationMiddlewareResultHandler
{
    public Task HandleAsync(
        RequestDelegate next,
        HttpContext context,
        AuthorizationPolicy policy,
        PolicyAuthorizationResult authorizeResult
    )
    {
        if (
            (authorizeResult.Challenged || authorizeResult.Forbidden)
            && context.GetEndpoint()?.Metadata.GetMetadata<StellarAdminEndpointMetadata>()
                is not null
            && context.Request.Headers["HX-Request"] == "true"
            && context.Request.Headers["HX-Boosted"] != "true"
        )
        {
            context.Response.Headers["HX-Refresh"] = "true";
            context.Response.StatusCode = StatusCodes.Status204NoContent;

            return Task.CompletedTask;
        }

        return inner.HandleAsync(next, context, policy, authorizeResult);
    }

    // Wraps the registered handler, the framework's unless the app registered its own first; the
    // registered handler moves to a keyed service with the same lifetime
    public static void Decorate(IServiceCollection services)
    {
        var existing = services.LastOrDefault(descriptor =>
            descriptor.ServiceType == typeof(IAuthorizationMiddlewareResultHandler)
            && !descriptor.IsKeyedService
        );
        if (existing?.ImplementationType == typeof(HtmxAuthorizationResultHandler))
        {
            return;
        }

        var key = typeof(HtmxAuthorizationResultHandler);
        var serviceType = typeof(IAuthorizationMiddlewareResultHandler);
        var lifetime = existing?.Lifetime ?? ServiceLifetime.Transient;
        if (existing is not null)
        {
            services.Remove(existing);
        }

        services.Add(
            existing switch
            {
                { ImplementationInstance: { } instance } => new ServiceDescriptor(
                    serviceType,
                    key,
                    instance
                ),
                { ImplementationFactory: { } factory } => new ServiceDescriptor(
                    serviceType,
                    key,
                    (provider, _) => factory(provider),
                    lifetime
                ),
                { ImplementationType: { } type } => new ServiceDescriptor(
                    serviceType,
                    key,
                    type,
                    lifetime
                ),
                _ => new ServiceDescriptor(
                    serviceType,
                    key,
                    typeof(AuthorizationMiddlewareResultHandler),
                    lifetime
                ),
            }
        );
        services.Add(
            new ServiceDescriptor(serviceType, typeof(HtmxAuthorizationResultHandler), lifetime)
        );
    }
}
