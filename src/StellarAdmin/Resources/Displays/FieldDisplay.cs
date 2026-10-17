namespace StellarAdmin.Resources.Displays;

/// <summary>
///     Configures how a field shows its value.
/// </summary>
/// <remarks>
///     Derive from this class to add settings. Render them in the display template that the
///     derived class names, which inherits <c>FieldDisplayView&lt;TDisplay&gt;</c>.
/// </remarks>
public abstract class FieldDisplay
{
    /// <summary>
    ///     Gets the name of the display template.
    /// </summary>
    /// <value>
    ///     The template name. The template is found at <c>DisplayTemplates/{name}</c>, and at
    ///     <c>GridDisplayTemplates/{name}</c> in index grid cells.
    /// </value>
    public abstract string TemplateName { get; }

    /// <summary>
    ///     Gets a value that indicates whether the template renders empty values itself.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> if the template renders empty values; otherwise,
    ///     <see langword="false" /> to show the field's empty text. The default is
    ///     <see langword="false" />.
    /// </value>
    /// <remarks>
    ///     Override it when the display shows something of its own for an empty value, such as a
    ///     placeholder.
    /// </remarks>
    public virtual bool RendersEmptyValue => false;
}
