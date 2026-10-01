using System.ComponentModel.DataAnnotations;

namespace DashboardPlayground.Resources.FieldEditors;

// Each property is one radio group scenario, and its display name describes the scenario.
public sealed class RadioGroupGallery
    : FieldEditorGalleryRecord,
        IFieldEditorGalleryRecord<RadioGroupGallery>
{
    // Inferred choices

    [Display(Name = "enum")]
    public GalleryCabin Cabin { get; set; }

    [Display(Name = "enum · Appearance = Cards")]
    public GalleryCabin CabinCards { get; set; }

    [Display(Name = "enum?")]
    public GalleryCabin? OptionalCabin { get; set; }

    [Required]
    [Display(Name = "enum? · [Required] · Cards")]
    public GalleryCabin? RequiredCabin { get; set; }

    [Display(Name = "bool")]
    public bool Insured { get; set; }

    // Field configuration

    [Display(Name = "enum · this display name is replaced by the field Title")]
    public GalleryCabin TitledCabin { get; set; }

    [Display(
        Name = "enum · [Display(Description)] replaced by field Description",
        Description = "This attribute description should not appear."
    )]
    public GalleryCabin DescribedCabin { get; set; }

    [Editable(false)]
    [Display(Name = "enum · [Editable(false)]")]
    public GalleryCabin ReadOnlyCabin { get; set; }

    [Editable(false)]
    [Display(Name = "enum · [Editable(false)] · Cards")]
    public GalleryCabin ReadOnlyCabinCards { get; set; }

    // UseItems and class names

    [Required]
    [Display(Name = "string · [Required] · UseItems")]
    public string? Seat { get; set; }

    [Display(Name = "string · UseItems · Cards · ClassNames.Option.Root")]
    public string? SeatCards { get; set; }

    public static RadioGroupGallery CreateSample() =>
        new()
        {
            Cabin = GalleryCabin.PremiumEconomy,
            CabinCards = GalleryCabin.Business,
            OptionalCabin = null,
            RequiredCabin = GalleryCabin.Economy,
            Insured = true,
            TitledCabin = GalleryCabin.First,
            DescribedCabin = GalleryCabin.Economy,
            ReadOnlyCabin = GalleryCabin.Business,
            ReadOnlyCabinCards = GalleryCabin.PremiumEconomy,
            Seat = "aisle",
            SeatCards = "window",
        };
}
