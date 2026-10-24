using System.ComponentModel.DataAnnotations;

namespace AdminPlayground.Resources.FieldEditors;

// Each property is one icon editor scenario, and its display name describes the scenario. Values are
// icon names; "spa-flower" isn't a registered icon, to show a stored value the editor doesn't offer.
public sealed class IconGallery : FieldEditorGalleryRecord, IFieldEditorGalleryRecord<IconGallery>
{
    [Display(Name = "Layout Auto · every icon (picker)")]
    public string? Icon { get; set; }

    [Required]
    [Display(
        Name = "Layout Auto · every icon · [Required]",
        Description = "Shown on the tour card."
    )]
    public string? TourIcon { get; set; }

    [Display(Name = "Layout Auto · 13 icons (tiles)")]
    public string? Amenity { get; set; }

    [Display(Name = "Layout Auto · 32 icons (picker)")]
    public string? Activity { get; set; }

    [Display(Name = "Layout Picker · 13 icons")]
    public string? PickerAmenity { get; set; }

    [Display(Name = "Layout Inline · 4 icons · [Required]")]
    [Required]
    public string? Transport { get; set; }

    [Display(Name = "Stored value isn't an icon · every icon")]
    public string? LegacyIcon { get; set; }

    [Display(Name = "Stored value isn't an icon · tiles")]
    public string? LegacyAmenity { get; set; }

    [Editable(false)]
    [Display(Name = "[Editable(false)]")]
    public string? ReadOnlyIcon { get; set; }

    [Display(Name = "Disabled()")]
    public string? DisabledIcon { get; set; }

    public static IconGallery CreateSample() =>
        new()
        {
            Icon = "compass",
            TourIcon = "mountain",
            Amenity = "coffee",
            Activity = "sailboat",
            PickerAmenity = "waves-ladder",
            Transport = "train-front",
            LegacyIcon = "spa-flower",
            LegacyAmenity = "spa-flower",
            ReadOnlyIcon = "wifi",
            DisabledIcon = "wifi",
        };
}
