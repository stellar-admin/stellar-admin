using StellarAdmin.TagHelpers;

namespace StellarAdmin.Dashboard.Resources.Options;

/// <summary>
///     Additional CSS classes for the parts of a form editor.
/// </summary>
public class EditorClassNames : FieldClassNames
{
    /// <summary>
    ///     Additional CSS classes for the visible control.
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
