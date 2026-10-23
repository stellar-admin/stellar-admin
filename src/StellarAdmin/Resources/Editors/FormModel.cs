using StellarAdmin.Diagnostics;

namespace StellarAdmin.Resources.Editors;

// The form an editor typed by its form's model loads its items with. A form of another type is a configuration mistake
// that the page reports for the field.
internal static class FormModel
{
    public static TModel Cast<TModel>(object model, string editorName) =>
        model is TModel typed
            ? typed
            : throw new ConfigurationException(
                ConfigurationProblemKind.UnsuitableEditor,
                $"the `{editorName}` loads its items from a `{typeof(TModel).Name}`, but the form edits a `{model.GetType().Name}`",
                $"Configure the field's editor on the `{model.GetType().Name}` form's fields."
            );
}
