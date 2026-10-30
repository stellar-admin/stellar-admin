namespace StellarAdmin.Areas.StellarAdmin.ViewModels.Internal;

// A resource page's widgets by placement. A placement no widget is configured for is null, so the page leaves out its
// region; one whose widgets the user doesn't see is empty, so the index page's region is there for a search to fill.
internal sealed record PageWidgetsViewModel(
    IReadOnlyList<WidgetViewModel>? Top,
    IReadOnlyList<WidgetViewModel>? Aside,
    IReadOnlyList<WidgetViewModel>? Bottom
);
