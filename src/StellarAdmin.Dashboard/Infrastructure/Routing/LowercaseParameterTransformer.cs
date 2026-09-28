using Microsoft.AspNetCore.Routing;

namespace StellarAdmin.Dashboard.Infrastructure.Routing;

// Lowercases route values written into generated Dashboard URLs. Incoming requests still
// match case-insensitively, so existing PascalCase links keep resolving.
internal sealed class LowercaseParameterTransformer : IOutboundParameterTransformer
{
    public const string Name = "stellaradmin-lowercase";

    public string? TransformOutbound(object? value) => value?.ToString()?.ToLowerInvariant();
}
