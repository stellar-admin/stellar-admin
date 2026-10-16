namespace StellarAdmin.Resources.Displays;

/// <summary>
///     Configures how a read-only form field renders its value.
/// </summary>
/// <remarks>
///     Derive from this class to add settings, and render them in the display template the
///     subclass names, which inherits <c>FieldDisplayView&lt;TDisplay&gt;</c>.
/// </remarks>
public abstract class FieldDisplay
{
    /// <summary>
    ///     The name of the display template, resolved as <c>DisplayTemplates/{name}</c>.
    /// </summary>
    public abstract string TemplateName { get; }

    /// <summary>
    ///     Whether the template renders empty values itself, instead of the field's empty text.
    /// </summary>
    /// <remarks>
    ///     Defaults to <c>false</c>. Override it when the display shows something of its own for
    ///     an empty value, such as a placeholder.
    /// </remarks>
    public virtual bool RendersEmptyValue => false;
}
