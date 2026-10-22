using StellarAdmin.Diagnostics;

namespace StellarAdmin.Resources.Editors;

// The form an items loader declared its model type for. A loader on a form of another type is a configuration mistake
// that the page reports for the field.
internal static class FormModel
{
    public static TModel Cast<TModel>(object model, Type editorType) =>
        model is TModel typed
            ? typed
            : throw new ConfigurationException(
                ConfigurationProblemKind.UnsuitableEditor,
                $"the `{editorType.Name}` loads its items from a `{typeof(TModel).Name}`, but the form edits a `{model.GetType().Name}`",
                $"Declare the form's model type, `{model.GetType().Name}`, in the items' delegate."
            );
}
