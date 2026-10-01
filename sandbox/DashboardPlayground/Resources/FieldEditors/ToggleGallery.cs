using System.ComponentModel.DataAnnotations;

namespace DashboardPlayground.Resources.FieldEditors;

// Each property is one toggle scenario, and its display name describes the scenario.
public sealed class ToggleGallery
    : FieldEditorGalleryRecord,
        IFieldEditorGalleryRecord<ToggleGallery>
{
    // States

    [Display(Name = "bool · on")]
    public bool Notifications { get; set; }

    [Display(Name = "bool · off")]
    public bool Newsletter { get; set; }

    [Display(Name = "bool · [Display(Description)]", Description = "Shown below the switch.")]
    public bool DescribedNotifications { get; set; }

    // Validation

    [Range(typeof(bool), "true", "true", ErrorMessage = "Turn on alerts to continue.")]
    [Display(Name = "bool · [Range(true, true)]")]
    public bool RequiredAlerts { get; set; }

    [Range(typeof(bool), "true", "true", ErrorMessage = "Turn on tracking to continue.")]
    [Display(
        Name = "bool · [Range(true, true)] · [Display(Description)]",
        Description = "The error appears with the description."
    )]
    public bool RequiredTracking { get; set; }

    // Field configuration

    [Display(Name = "bool · this display name is replaced by the field Title")]
    public bool TitledNotifications { get; set; }

    [Display(Name = "bool · field Description")]
    public bool FieldDescribedNotifications { get; set; }

    [Editable(false)]
    [Display(Name = "bool · [Editable(false)] · on")]
    public bool ReadOnlyOn { get; set; }

    [Editable(false)]
    [Display(Name = "bool · [Editable(false)] · off")]
    public bool ReadOnlyOff { get; set; }

    // Class names

    [Display(Name = "bool · ClassNames.Label")]
    public bool LabelStyled { get; set; }

    [Display(
        Name = "bool · ClassNames.Root",
        Description = "The root wraps the switch and its text."
    )]
    public bool RootStyled { get; set; }

    public static ToggleGallery CreateSample() =>
        new()
        {
            Notifications = true,
            Newsletter = false,
            DescribedNotifications = true,
            RequiredAlerts = true,
            RequiredTracking = true,
            TitledNotifications = false,
            FieldDescribedNotifications = true,
            ReadOnlyOn = true,
            ReadOnlyOff = false,
            LabelStyled = true,
            RootStyled = false,
        };
}
