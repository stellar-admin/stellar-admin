using System.ComponentModel.DataAnnotations;
using StellarAdmin.Resources.Editors;

namespace StellarAdmin.Resources.Options;

// A field without an editor whose property is a list of strings or numbers gets the list editor, and one whose property
// is a dictionary of them the dictionary editor, so that their items are checked again as the form is saved. A field
// with a binder edits the binder's value, and a [UIHint] names the template the app wants.
internal static class CollectionEditorCheck
{
    public static void Run<TResource>(ResourceOptions<TResource> options)
    {
        CheckFields(options.Create);
        CheckFields(options.Edit);
    }

    private static void CheckFields(ResourceFormOptions? formOptions)
    {
        foreach (var field in FormItems.Fields(formOptions))
        {
            var property = field.PropertyPath[^1];
            if (
                field is not { Editor: null, Binder: null }
                || property.IsDefined(typeof(UIHintAttribute), inherit: true)
            )
            {
                continue;
            }

            FieldEditor? editor =
                CollectionEditorTypes.ListElementType(property.PropertyType) is not null
                    ? new ListEditor()
                : CollectionEditorTypes.DictionaryTypes(property.PropertyType) is not null
                    ? new DictionaryEditor()
                : null;
            if (editor is not null)
            {
                field.Editor = editor;
                field.HandlerType = (editor as IFieldEditor)?.HandlerType;
            }
        }
    }
}
