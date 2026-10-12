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
    /// <summary>
    ///     The field's display configuration, or default settings when the template renders
    ///     without one.
    /// </summary>
    public TDisplay Display =>
        field ??= ViewData[ViewDataKeys.FieldDisplay] as TDisplay ?? new TDisplay();
}
