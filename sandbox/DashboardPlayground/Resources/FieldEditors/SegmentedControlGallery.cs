using System.ComponentModel.DataAnnotations;

namespace DashboardPlayground.Resources.FieldEditors;

// Each property is one segmented control scenario, and its display name describes the scenario.
public sealed class SegmentedControlGallery
    : FieldEditorGalleryRecord,
        IFieldEditorGalleryRecord<SegmentedControlGallery>
{
    [Display(Name = "enum")]
    public GalleryCabin Cabin { get; set; }

    [Display(Name = "enum?")]
    public GalleryCabin? OptionalCabin { get; set; }

    [Display(Name = "bool")]
    public bool Insured { get; set; }

    [Required]
    [Display(Name = "string · [Required] · UseItems")]
    public string? Seat { get; set; }

    [Display(Name = "enum · this display name is replaced by the field Title")]
    public GalleryCabin TitledCabin { get; set; }

    [Display(Name = "enum · [Display(Description)]", Description = "Shown below the buttons.")]
    public GalleryCabin DescribedCabin { get; set; }

    [Display(Name = "enum · field Description")]
    public GalleryCabin FieldDescribedCabin { get; set; }

    [Editable(false)]
    [Display(Name = "enum · [Editable(false)]")]
    public GalleryCabin ReadOnlyCabin { get; set; }

    [Display(Name = "enum · ClassNames.Control")]
    public GalleryCabin StyledCabin { get; set; }

    public static SegmentedControlGallery CreateSample() =>
        new()
        {
            Cabin = GalleryCabin.Business,
            OptionalCabin = null,
            Insured = false,
            Seat = "middle",
            TitledCabin = GalleryCabin.Economy,
            DescribedCabin = GalleryCabin.PremiumEconomy,
            FieldDescribedCabin = GalleryCabin.First,
            ReadOnlyCabin = GalleryCabin.First,
            StyledCabin = GalleryCabin.Economy,
        };
}
