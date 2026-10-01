using System.ComponentModel.DataAnnotations;

namespace DashboardPlayground.Resources.FieldEditors;

// Each property is one checkbox scenario, and its display name describes the scenario.
public sealed class CheckboxGallery
    : FieldEditorGalleryRecord,
        IFieldEditorGalleryRecord<CheckboxGallery>
{
    // Data-type template

    [Display(Name = "bool")]
    public bool Subscribed { get; set; }

    [Display(Name = "bool · [Display(Description)]", Description = "Shown below the checkbox.")]
    public bool DescribedSubscribed { get; set; }

    [Display(Name = "bool · UseEditor<CheckboxEditor>")]
    public bool ExplicitSubscribed { get; set; }

    // Validation

    [Range(typeof(bool), "true", "true", ErrorMessage = "Accept the terms to continue.")]
    [Display(Name = "bool · [Range(true, true)]")]
    public bool AcceptedTerms { get; set; }

    [Range(typeof(bool), "true", "true", ErrorMessage = "Confirm the passport is valid.")]
    [Display(
        Name = "bool · [Range(true, true)] · [Display(Description)]",
        Description = "The error appears with the description."
    )]
    public bool PassportConfirmed { get; set; }

    // Field configuration

    [Display(Name = "bool · this display name is replaced by the field Title")]
    public bool TitledSubscribed { get; set; }

    [Display(Name = "bool · field Description")]
    public bool FieldDescribedSubscribed { get; set; }

    [Editable(false)]
    [Display(Name = "bool · [Editable(false)] · checked")]
    public bool ReadOnlyChecked { get; set; }

    [Editable(false)]
    [Display(Name = "bool · [Editable(false)] · unchecked")]
    public bool ReadOnlyUnchecked { get; set; }

    // Class names

    [Display(Name = "bool · ClassNames.Label")]
    public bool LabelStyled { get; set; }

    [Display(
        Name = "bool · ClassNames.Root",
        Description = "The root wraps the checkbox and its text."
    )]
    public bool RootStyled { get; set; }

    public static CheckboxGallery CreateSample() =>
        new()
        {
            Subscribed = true,
            DescribedSubscribed = false,
            ExplicitSubscribed = true,
            AcceptedTerms = true,
            PassportConfirmed = true,
            TitledSubscribed = true,
            FieldDescribedSubscribed = false,
            ReadOnlyChecked = true,
            ReadOnlyUnchecked = false,
            LabelStyled = true,
            RootStyled = false,
        };
}
