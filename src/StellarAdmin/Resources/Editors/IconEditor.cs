using StellarAdmin.UI;

namespace StellarAdmin.Resources.Editors;

/// <summary>
///     Configures an icon editor for a property that stores the name of an icon.
/// </summary>
/// <remarks>
///     A new value must be a registered icon, and one of <see cref="Icons" /> when the field has a
///     list. A stored value that isn't saves unchanged.
/// </remarks>
public sealed class IconEditor : FieldEditor
{
    // A list this long or shorter shows as tiles with the automatic layout
    internal const int InlineIconLimit = 24;

    /// <summary>
    ///     Gets or sets the icons the user can choose from.
    /// </summary>
    /// <value>
    ///     The names of registered icons, or <see langword="null" /> to offer every registered
    ///     icon. The default is <see langword="null" />.
    /// </value>
    /// <remarks>
    ///     The list must not be empty, and each name must be a registered icon.
    /// </remarks>
    public IReadOnlyList<string>? Icons { get; set; }

    /// <summary>
    ///     Gets or sets how the editor shows the icons on a form.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the layout. The default is
    ///     <see cref="IconEditorLayout.Auto" />.
    /// </value>
    /// <remarks>
    ///     In an inline row, the editor is always a button that opens the picker.
    /// </remarks>
    public IconEditorLayout Layout { get; set; } = IconEditorLayout.Auto;

    /// <inheritdoc />
    public override string TemplateName => "Editors/Icon";

    internal IconPickerLayout ResolveLayout() =>
        Layout switch
        {
            IconEditorLayout.Inline => IconPickerLayout.Inline,
            IconEditorLayout.Auto when Icons is { Count: > 0 and <= InlineIconLimit } =>
                IconPickerLayout.Inline,
            _ => IconPickerLayout.Picker,
        };
}
