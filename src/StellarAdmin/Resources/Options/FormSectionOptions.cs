using StellarAdmin.UI;

namespace StellarAdmin.Resources.Options;

/// <summary>
///     Stores the configuration of a titled form section.
/// </summary>
public sealed class FormSectionOptions : FormContainerOptions
{
    /// <summary>
    ///     Gets the supporting text displayed below the section's title.
    /// </summary>
    public string? Description { get; internal set; }

    /// <summary>
    ///     Gets the layout of the section.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the layout, or <see langword="null" /> to
    ///     use the form's section layout.
    /// </value>
    public FormSectionLayout? Layout { get; internal set; }

    /// <summary>
    ///     Gets whether the labels of the section's read-only fields display above or beside their
    ///     values.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the orientation, or
    ///     <see langword="null" /> to use the orientation of the parent section.
    /// </value>
    /// <remarks>
    ///     A section without a parent displays labels above their values.
    /// </remarks>
    public FieldOrientation? Orientation { get; internal set; }

    /// <summary>
    ///     Gets the section's title.
    /// </summary>
    public string Title { get; internal set; }

    internal FormSectionOptions(string title)
    {
        Title = title;
    }
}
