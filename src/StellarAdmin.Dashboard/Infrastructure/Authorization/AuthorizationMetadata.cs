using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace StellarAdmin.Dashboard.Infrastructure.Authorization;

// Translates the RequireAuthorization overloads into the endpoint metadata the framework's
// RequireAuthorization extension adds, so the authorization middleware evaluates both alike.
internal static class AuthorizationMetadata
{
    // Evaluates metadata the way the authorization middleware does, for UI that should only
    // offer pages the current user can open. It uses the request's current user and does not
    // authenticate policy-specific schemes, since that would replace the rendering page's user.
    public static async Task<bool> AuthorizeAsync(
        IReadOnlyList<object> metadata,
        HttpContext httpContext
    )
    {
        if (metadata.Count == 0)
        {
            return true;
        }

        var services = httpContext.RequestServices;
        var policy = await AuthorizationPolicy.CombineAsync(
            services.GetRequiredService<IAuthorizationPolicyProvider>(),
            metadata.OfType<IAuthorizeData>(),
            metadata.OfType<AuthorizationPolicy>()
        );
        if (policy is null)
        {
            return true;
        }

        var result = await services
            .GetRequiredService<IAuthorizationService>()
            .AuthorizeAsync(httpContext.User, httpContext, policy);

        return result.Succeeded;
    }

    public static object[] Create(AuthorizationPolicy policy)
    {
        ArgumentNullException.ThrowIfNull(policy);

        return [new AuthorizeAttribute(), policy];
    }

    public static object[] Create(Action<AuthorizationPolicyBuilder> configurePolicy)
    {
        ArgumentNullException.ThrowIfNull(configurePolicy);

        var policy = new AuthorizationPolicyBuilder();
        configurePolicy(policy);

        return Create(policy.Build());
    }

    public static object[] Create(IAuthorizeData[] authorizeData)
    {
        ArgumentNullException.ThrowIfNull(authorizeData);

        return authorizeData.Length == 0 ? [new AuthorizeAttribute()] : [.. authorizeData];
    }

    public static object[] Create(string[] policyNames)
    {
        ArgumentNullException.ThrowIfNull(policyNames);

        return Create(
            policyNames.Select(IAuthorizeData (name) => new AuthorizeAttribute(name)).ToArray()
        );
    }
}
