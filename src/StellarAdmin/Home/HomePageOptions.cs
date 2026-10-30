using StellarAdmin.Diagnostics;
using StellarAdmin.Widgets;

namespace StellarAdmin.Home;

internal sealed class HomePageOptions
{
    public const string Page = "Home page";

    // The widgets are checked once, when the problems are listed or the page first renders
    private readonly Lazy<List<ConfigurationProblem>> _widgetProblems;

    public bool ShowSetupChecklist { get; set; } = true;

    public IReadOnlyList<ConfigurationProblem> WidgetProblems => _widgetProblems.Value;

    // The widgets as configured, then, once checked, those that can be shown
    public List<WidgetOptions> Widgets { get; } = [];

    public HomePageOptions() => _widgetProblems = new(() => WidgetsCheck.Run(Widgets, Page, false));

    public IReadOnlyList<WidgetOptions> GetCheckedWidgets()
    {
        _ = _widgetProblems.Value;

        return Widgets;
    }
}
