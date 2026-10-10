namespace StellarAdmin.Dashboard;

internal static class DashboardThemeCatalog
{
    public static DashboardThemeAssets Get(DashboardThemePreset preset) =>
        preset switch
        {
            DashboardThemePreset.Default => new(null, "Inter:wght@400..700"),
            DashboardThemePreset.Ledger => new("ledger", "Lexend:wght@300..700"),
            DashboardThemePreset.Ops => new("ops", "IBM+Plex+Sans:wght@400;500;600;700"),
            DashboardThemePreset.Soft => new("soft", "Figtree:wght@400..700"),
            _ => throw new ArgumentOutOfRangeException(nameof(preset)),
        };
}

internal readonly record struct DashboardThemeAssets(string? Preset, string FontFamilies);
