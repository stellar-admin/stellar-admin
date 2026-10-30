namespace StellarAdmin.Areas.StellarAdmin.ViewModels.Internal;

internal enum WidgetState
{
    Loading,
    Loaded,
    Failed,
}

// A widget's cell in the grid. Url loads the widget again, with its parameters; a widget without a handler has none.
internal sealed record WidgetViewModel(string Name, string PartialName, int Span, WidgetState State)
{
    public object? Model { get; init; }

    public TimeSpan? RefreshInterval { get; init; }

    public string? Url { get; init; }
}
