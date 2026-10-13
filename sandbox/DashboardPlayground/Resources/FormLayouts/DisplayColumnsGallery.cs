using DashboardPlayground.Resources.FieldEditors;

namespace DashboardPlayground.Resources.FormLayouts;

// The display fields gallery's booking again, with its index showing configured column displays. A
// resource type has one index, so the settings need a type of their own.
public sealed class DisplayColumnsGallery
    : DisplayFieldsLayoutGallery,
        IFieldEditorGalleryRecord<DisplayColumnsGallery>
{
    public static new DisplayColumnsGallery CreateSample() =>
        (DisplayColumnsGallery)new DisplayColumnsGallery().WithSample();
}
