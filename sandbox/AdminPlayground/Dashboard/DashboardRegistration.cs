using StellarAdmin;

namespace AdminPlayground.Dashboard;

internal static class DashboardRegistration
{
    // The home page's widgets: stats loaded after the page, a partial that renders with it, a widget with a picker,
    // and one that refreshes
    internal static void AddHomeDashboard(this StellarAdminBuilder admin)
    {
        admin.ConfigureHomePage(home =>
            home.Widgets(widgets =>
            {
                widgets.Add<CatalogStatsHandler>(
                    "Catalog",
                    "_CatalogWidget",
                    widget => widget.Span = 8
                );
                widgets.Add("Quick links", "_QuickLinksWidget", widget => widget.Span = 4);
                widgets.Add<TopCategoriesHandler>(
                    "Top categories",
                    "_TopCategoriesWidget",
                    widget =>
                    {
                        widget.Span = 6;
                        widget.Parameters(_ => new TopCategoriesParameters(Count: 5));
                    }
                );
                widgets.Add<TeamHandler>(
                    "Team",
                    "_TeamWidget",
                    widget =>
                    {
                        widget.Span = 6;
                        widget.RefreshInterval = TimeSpan.FromSeconds(30);
                    }
                );
            })
        );
    }
}
