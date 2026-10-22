using StellarAdmin.Diagnostics;

namespace StellarAdmin.Resources.Options;

// A field whose value another field's items depend on. The parent is a field of the same form, found by its name.
internal sealed class FieldDependency(string parentFieldName)
{
    // The disabled dependent's empty text while the parent has no value, or null for the generated text
    public string? EmptyText { get; set; }

    // The parent's field, once the form's fields are checked
    public FormFieldOptions? Parent { get; set; }

    public string ParentFieldName { get; } = parentFieldName;

    public ConfigurationSource? Source { get; init; }
}
