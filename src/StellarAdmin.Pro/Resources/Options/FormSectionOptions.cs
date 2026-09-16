namespace StellarAdmin.Pro.Resources.Options;

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
    ///     The section title.
    /// </summary>
    public string Title { get; internal set; }

    internal FormSectionOptions(string title)
    {
        Title = title;
    }
}
