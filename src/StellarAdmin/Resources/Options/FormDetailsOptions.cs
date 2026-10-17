using StellarAdmin.UI;

namespace StellarAdmin.Resources.Options;

/// <summary>
///     Stores the configuration of a titled form section that lists read-only fields.
/// </summary>
public sealed class FormDetailsOptions : FormItemOptions
{
    /// <summary>
    ///     Gets the supporting text displayed below the section's title.
    /// </summary>
    public string? Description { get; internal set; }

    /// <summary>
    ///     Gets the section's read-only fields, in display order.
    /// </summary>
    public IReadOnlyList<FormDisplayFieldOptions> Fields => MutableFields;

    /// <summary>
    ///     Gets the layout of the section.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the layout, or <see langword="null" /> to
    ///     use the form's section layout.
    /// </value>
    public FormSectionLayout? Layout { get; internal set; }

    /// <summary>
    ///     Gets whether labels display above or beside their values.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the orientation, or
    ///     <see langword="null" /> to display labels above their values.
    /// </value>
    public DetailsOrientation? Orientation { get; internal set; }

    /// <summary>
    ///     Gets the section's title.
    /// </summary>
    public string Title { get; internal set; }

    /// <summary>
    ///     Gets the visual style of the list.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the style, or <see langword="null" /> to
    ///     use <see cref="DetailsVariant.Default" />.
    /// </value>
    public DetailsVariant? Variant { get; internal set; }

    internal FormGridColumnDefinitions Columns { get; set; }

    internal List<FormDisplayFieldOptions> MutableFields { get; } = [];

    internal FormDetailsOptions(string title)
    {
        Title = title;
    }
}
