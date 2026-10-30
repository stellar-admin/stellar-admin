using StellarAdmin.Diagnostics;

namespace StellarAdmin.Widgets;

internal sealed class WidgetOptions(string name, string partialName, WidgetLoader? loader)
{
    public const int DefaultSpan = 12;

    public List<object> AuthorizationMetadata { get; } = [];

    public bool Inline { get; set; }

    // Null for a widget without a handler, which renders with the page
    public WidgetLoader? Loader { get; } = loader;

    public string Name { get; } = name;

    // Returns the parameters for the page's context, typed as ParametersType
    public Func<WidgetContext, object?>? Parameters { get; set; }

    public Type? ParametersType { get; set; }

    public string PartialName { get; } = partialName;

    public WidgetPlacement Placement { get; set; }

    public TimeSpan? RefreshInterval { get; set; }

    public Func<WidgetContext, bool>? ShowWhen { get; set; }

    public ConfigurationSource? Source { get; init; }

    public int Span { get; set; } = DefaultSpan;
}
