namespace StellarAdmin.Home;

internal sealed record SetupStatus(
    bool HasAuthentication,
    int ResourceCount,
    bool IsProtected,
    bool HasBranding,
    bool HasUserMenu,
    string? ThemeStylesheet,
    bool HasSidebarLayout,
    EntityCandidates Entities
)
{
    public bool HasTheme => ThemeStylesheet is not null;
}
