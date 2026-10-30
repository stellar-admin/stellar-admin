namespace StellarAdmin.Infrastructure.Authorization;

// Marks the endpoints MapStellarAdmin maps, so app-wide services can tell the admin panel's requests apart
internal sealed class StellarAdminEndpointMetadata
{
    public static readonly StellarAdminEndpointMetadata Instance = new();

    private StellarAdminEndpointMetadata() { }
}
