using System.ComponentModel.DataAnnotations;

namespace DashboardPlayground.Resources.FieldEditors;

// A gallery resource's single record. Create pages start from the empty instance and edit pages
// show the sample, so both the empty and the filled states of every scenario are visible.
public interface IFieldEditorGalleryRecord<TSelf>
    where TSelf : FieldEditorGalleryRecord, IFieldEditorGalleryRecord<TSelf>
{
    static abstract TSelf CreateSample();
}

// The options every gallery form shows above its scenarios. They are bound like any other field,
// so a choice survives a rejected save.
public abstract class FieldEditorGalleryRecord
{
    public int Id { get; set; }

    [Display(
        Name = "Reject every field",
        Description = "A save that passes validation is rejected with an error on every field."
    )]
    public bool RejectEveryField { get; set; }

    [Display(
        Name = "Skip client validation",
        Description = "Submits without the browser's checks, so invalid values reach server validation."
    )]
    public bool SkipClientValidation { get; set; }
}
