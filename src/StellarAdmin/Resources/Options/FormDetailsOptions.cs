using StellarAdmin.UI;

namespace StellarAdmin.Resources.Options;

/// <summary>
///     A titled section of a form that lists read-only fields.
/// </summary>
public sealed class FormDetailsOptions : FormItemOptions
{
    /// <summary>
    ///     Supporting text displayed below the section title.
    /// </summary>
    public string? Description { get; internal set; }

    /// <summary>
    ///     The section's read-only fields, in display order.
    /// </summary>
    public IReadOnlyList<FormDisplayFieldOptions> Fields => MutableFields;

    /// <summary>
    ///     The layout of the section.
    /// </summary>
    /// <remarks>
    ///     Defaults to the form's section layout.
    /// </remarks>
    public FormSectionLayout? Layout { get; internal set; }

    /// <summary>
    ///     Whether labels display above or beside their values.
    /// </summary>
    /// <remarks>
    ///     Defaults to above.
    /// </remarks>
    public DetailsOrientation? Orientation { get; internal set; }

    /// <summary>
    ///     The section title.
    /// </summary>
    public string Title { get; internal set; }

    /// <summary>
    ///     The visual variant of the list.
    /// </summary>
    /// <remarks>
    ///     Defaults to <see cref="DetailsVariant.Default" />.
    /// </remarks>
    public DetailsVariant? Variant { get; internal set; }

    internal FormGridColumnDefinitions Columns { get; set; }

    internal List<FormDisplayFieldOptions> MutableFields { get; } = [];

    internal FormDetailsOptions(string title)
    {
        Title = title;
    }
}
