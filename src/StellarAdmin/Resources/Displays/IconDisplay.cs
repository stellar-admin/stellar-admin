namespace StellarAdmin.Resources.Displays;

/// <summary>
///     Shows the icon that a field names.
/// </summary>
/// <remarks>
///     A name that isn't a registered icon shows a dashed box in the icon's place.
/// </remarks>
public sealed class IconDisplay : FieldDisplay
{
    /// <inheritdoc />
    public override string TemplateName => "Displays/Icon";

    /// <summary>
    ///     Gets or sets a value that indicates whether the icon's name shows beside it.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> to show the name; <see langword="false" /> to show it only on
    ///     hover; or <see langword="null" /> for the default. The default hides the name in the
    ///     grid and shows it on the view page.
    /// </value>
    public bool? ShowName { get; set; }
}
