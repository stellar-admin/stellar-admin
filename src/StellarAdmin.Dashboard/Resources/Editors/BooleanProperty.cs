using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace StellarAdmin.Dashboard.Resources.Editors;

internal static class BooleanProperty
{
    public static void Ensure(FieldEditor editor, ModelMetadata metadata)
    {
        if (metadata.ModelType != typeof(bool))
        {
            throw new InvalidOperationException(
                $"{editor.GetType().Name} on {metadata.PropertyName} requires a non-nullable Boolean property. Use SelectEditor, RadioGroupEditor or ToggleButtonsEditor for a nullable Boolean."
            );
        }
    }
}
