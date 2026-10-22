using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Resources;
using StellarAdmin.Resources.Displays;

namespace StellarAdmin.Areas.StellarAdmin;

/// <summary>
///     Provides the base class for display templates, with access to the field's display
///     settings.
/// </summary>
/// <typeparam name="TDisplay">The type of the display that the template renders.</typeparam>
/// <remarks>
///     The template's model is the field's value.
/// </remarks>
public abstract class FieldDisplayView<TDisplay> : RazorPage<object?>
    where TDisplay : FieldDisplay, new()
{
    private static readonly TDisplay DefaultDisplay = new();

    /// <summary>
    ///     Gets the field's display settings.
    /// </summary>
    /// <value>
    ///     The field's display, or a display with default settings when the field has none.
    /// </value>
    // Read on each access: a grid renders every cell with the same page instance, so a cached display would
    // carry over to the next column using the template
    public TDisplay Display => ViewData[ViewDataKeys.FieldDisplay] as TDisplay ?? DefaultDisplay;

    /// <summary>
    ///     Gets the data that the display's handler prepared for the current request, or
    ///     <see langword="null" /> when the display has no handler.
    /// </summary>
    public object? DisplayData => ViewData[ViewDataKeys.FieldDisplayData];

    /// <summary>
    ///     Formats the field's value in the current culture.
    /// </summary>
    /// <param name="format">
    ///     A format string, such as <c>N2</c>, or <see langword="null" /> to use the property's
    ///     <c>[DisplayFormat]</c>.
    /// </param>
    /// <returns>
    ///     The formatted value, or an empty string when the value is <see langword="null" />.
    /// </returns>
    /// <remarks>
    ///     Without a format string or <c>[DisplayFormat]</c>, enum members show their
    ///     <c>[Display]</c> names. A record of a registered resource shows its title.
    /// </remarks>
    public string FormatValue(string? format = null) =>
        FormatValue(Model, ViewData.ModelMetadata, format);

    // A value, or an item of a collection: a record of a registered resource shows its title
    internal string FormatValue(object? value, ModelMetadata metadata, string? format = null) =>
        format is null
        && value is not null
        && Context.RequestServices.GetRequiredService<ResourceReferences>().FormatTitle(value)
            is { } title
            ? title
            : FieldValueFormatter.Format(value, metadata, format);
}
