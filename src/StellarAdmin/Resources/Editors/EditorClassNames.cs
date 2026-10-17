using StellarAdmin.UI;

namespace StellarAdmin.Resources.Editors;

/// <summary>
///     Stores additional CSS classes for the parts of a form editor.
/// </summary>
public class EditorClassNames : FieldClassNames
{
    /// <summary>
    ///     Gets or sets additional CSS classes for the visible control.
    /// </summary>
    public string? Control { get; set; }

    internal void CopyTo(EditorClassNames target)
    {
        target.Content = Content;
        target.Control = Control;
        target.Description = Description;
        target.Error = Error;
        target.Label = Label;
        target.Root = Root;
    }
}
