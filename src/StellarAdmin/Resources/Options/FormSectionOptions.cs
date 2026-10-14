using StellarAdmin.UI;

namespace StellarAdmin.Resources.Options;

/// <summary>
///     A titled section of a form.
/// </summary>
public sealed class FormSectionOptions : FormContainerOptions
{
    /// <summary>
    ///     Supporting text displayed below the section title.
    /// </summary>
    public string? Description { get; internal set; }

    /// <summary>
    ///     The layout of the section.
    /// </summary>
    /// <remarks>
    ///     Defaults to the form's section layout.
    /// </remarks>
    public FormSectionLayout? Layout { get; internal set; }

    /// <summary>
    ///     Whether the labels of the section's display fields display above or beside their values.
    /// </summary>
    /// <remarks>
    ///     Defaults to the orientation of the section's parent section, then above.
    /// </remarks>
    public FieldOrientation? Orientation { get; internal set; }

    /// <summary>
    ///     The section title.
    /// </summary>
    public string Title { get; internal set; }

    internal FormSectionOptions(string title)
    {
        Title = title;
    }
}
