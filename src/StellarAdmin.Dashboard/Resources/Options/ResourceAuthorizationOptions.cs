namespace StellarAdmin.Dashboard.Resources.Options;

// Kept apart from ResourceOptions so endpoint construction can read it without resolving
// and validating the full resource configuration.
internal sealed class ResourceAuthorizationOptions<TResource>
{
    public List<object> Metadata { get; } = [];
}
