using Microsoft.AspNetCore.Mvc.ModelBinding;
using StellarAdmin.Resources.Options;

namespace StellarAdmin.Areas.StellarAdmin;

// Whether a field is read-only or disabled for a record. The property's metadata also makes a field read-only, as the
// editor templates read it, except for a field with a binder, which edits the binder's value. Disabled wins.
internal static class FieldStates
{
    // A field the form binds a posted value for
    public static bool IsEditable(
        FormFieldOptions field,
        object model,
        IModelMetadataProvider metadataProvider
    ) => !field.IsDisabled(model) && !IsReadOnly(field, model, metadataProvider);

    public static bool IsReadOnly(
        FormFieldOptions field,
        object model,
        IModelMetadataProvider metadataProvider
    ) =>
        field.IsReadOnly(model)
        || field.Binder is null && GetMetadata(field, model, metadataProvider)?.IsReadOnly == true;

    // A grid row shows a read-only field's value in place of its editor, and a disabled field's editor disabled
    public static bool ShowsGridEditor(
        FormFieldOptions field,
        object model,
        IModelMetadataProvider metadataProvider
    ) => field.IsDisabled(model) || !IsReadOnly(field, model, metadataProvider);

    private static ModelMetadata? GetMetadata(
        FormFieldOptions field,
        object model,
        IModelMetadataProvider metadataProvider
    )
    {
        ModelMetadata? metadata = metadataProvider.GetMetadataForType(model.GetType());
        foreach (var property in field.PropertyPath)
        {
            metadata = metadata.Properties[property.Name];
            if (metadata is null)
            {
                return null;
            }
        }

        return metadata;
    }
}
