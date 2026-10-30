namespace StellarAdmin.Areas.StellarAdmin.ViewModels.Internal;

// The index page's widgets by placement. A placement no widget is configured for is null, so the page leaves out its
// region; one whose widgets the user doesn't see is empty, so its region is there for a search to fill.
internal sealed record IndexWidgetsViewModel(
    IReadOnlyList<WidgetViewModel>? Top,
    IReadOnlyList<WidgetViewModel>? Aside,
    IReadOnlyList<WidgetViewModel>? Bottom
);
