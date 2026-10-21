using Microsoft.AspNetCore.Mvc.ModelBinding;
using StellarAdmin.Diagnostics;

namespace StellarAdmin.Resources.Editors;

internal static class BooleanProperty
{
    public static void Ensure(FieldEditor editor, ModelMetadata metadata)
    {
        if (metadata.ModelType != typeof(bool))
        {
            throw new ConfigurationException(
                ConfigurationProblemKind.UnsuitableEditor,
                $"`{editor.GetType().Name}` needs a non-nullable Boolean property, and `{metadata.PropertyName}` isn't one",
                "Bind the field to a non-nullable Boolean property, or use `SelectEditor`, `RadioGroupEditor` or `SegmentedControlEditor` for a nullable Boolean."
            );
        }
    }
}
