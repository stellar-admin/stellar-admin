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
                $"{editor.GetType().Name} on {metadata.PropertyName} requires a non-nullable Boolean property. Use SelectEditor, RadioGroupEditor or SegmentedControlEditor for a nullable Boolean."
            );
        }
    }
}
