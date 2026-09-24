namespace StellarAdmin.Dashboard;

internal static class DashboardThemeCatalog
{
    public static DashboardThemeAssets Get(DashboardTheme theme) =>
        theme switch
        {
            DashboardTheme.Aurora => new(
                "aurora",
                "Archivo:wght@400;500;600&family=IBM+Plex+Mono:wght@400;500"
            ),
            DashboardTheme.Concourse => new(
                "concourse",
                "Source+Sans+3:wght@400;500;600;700&family=IBM+Plex+Mono:wght@400;500"
            ),
            DashboardTheme.Ice => new(
                "ice",
                "IBM+Plex+Sans:wght@400;500;600&family=JetBrains+Mono:wght@400;500;700"
            ),
            DashboardTheme.Ledger => new(
                "ledger",
                "Lexend:wght@300..700&family=JetBrains+Mono:wght@400;500"
            ),
            DashboardTheme.Meridian => new(
                "meridian",
                "Instrument+Sans:wght@600&family=Work+Sans:wght@400;500;600&family=JetBrains+Mono:wght@400;500"
            ),
            DashboardTheme.Observatory => new(
                "observatory",
                "IBM+Plex+Sans:wght@400;500;600&family=IBM+Plex+Mono:wght@400;500"
            ),
            DashboardTheme.Parallax => new(
                "parallax",
                "Space+Grotesk:wght@400;500;600&family=JetBrains+Mono:wght@400;500"
            ),
            DashboardTheme.ShadcnLuma => new("shadcn.luma", "Inter:wght@400..700"),
            DashboardTheme.ShadcnLyra => new("shadcn.lyra", "JetBrains+Mono:wght@400..700"),
            DashboardTheme.ShadcnMaia => new("shadcn.maia", "Figtree:wght@400..700"),
            DashboardTheme.ShadcnMira => new("shadcn.mira", "Inter:wght@400..700"),
            DashboardTheme.ShadcnNova => new("shadcn.nova", "Geist:wght@400..700"),
            DashboardTheme.ShadcnRhea => new("shadcn.rhea", "Inter:wght@400..700"),
            DashboardTheme.ShadcnSera => new(
                "shadcn.sera",
                "Noto+Sans:wght@400..700&family=Playfair+Display:wght@400..700"
            ),
            DashboardTheme.ShadcnVega => new("shadcn.vega", "Inter:wght@400..700"),
            _ => throw new ArgumentOutOfRangeException(nameof(theme)),
        };
}

internal readonly record struct DashboardThemeAssets(string Name, string FontFamilies);
