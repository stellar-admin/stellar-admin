using Microsoft.AspNetCore.Mvc.Razor;
using StellarAdmin.Dashboard.Resources.Displays;

namespace StellarAdmin.Dashboard.Areas.StellarAdmin;

/// <summary>
///     Base class for display templates, exposing the field's display configuration.
/// </summary>
/// <typeparam name="TDisplay">The display configuration the template renders.</typeparam>
/// <remarks>
///     The template's model is the field's value.
/// </remarks>
public abstract class FieldDisplayView<TDisplay> : RazorPage<object?>
    where TDisplay : FieldDisplay, new()
{
    private static readonly TDisplay DefaultDisplay = new();

    /// <summary>
    ///     The field's display configuration, or default settings when the template renders
    ///     without one.
    /// </summary>
    // Read on each access: a grid renders every cell with the same page instance, so a cached display would
    // carry over to the next column using the template
    public TDisplay Display => ViewData[ViewDataKeys.FieldDisplay] as TDisplay ?? DefaultDisplay;

    /// <summary>
    ///     Data prepared for this display on the current request, or <c>null</c> when the display
    ///     has no handler.
    /// </summary>
    public object? DisplayData => ViewData[ViewDataKeys.FieldDisplayData];

    /// <summary>
    ///     Formats the field's value in the current culture.
    /// </summary>
    /// <param name="format">
    ///     A format string, such as <c>N2</c>, or <c>null</c> to use the property's
    ///     <c>[DisplayFormat]</c>. Without either, enum members show their <c>[Display]</c> names.
    /// </param>
    public string FormatValue(string? format = null) =>
        FieldValueFormatter.Format(Model, ViewData.ModelMetadata, format);
}
