using Microsoft.AspNetCore.Authorization;

namespace StellarAdmin.Dashboard.Infrastructure.Authorization;

// Translates the RequireAuthorization overloads into the endpoint metadata the framework's
// RequireAuthorization extension adds, so the authorization middleware evaluates both alike.
internal static class AuthorizationMetadata
{
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
