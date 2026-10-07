using System.ComponentModel.DataAnnotations;

namespace DashboardPlayground.Resources.FieldEditors;

// Each property is one select scenario, and its display name describes the scenario.
public sealed class SelectGallery
    : FieldEditorGalleryRecord,
        IFieldEditorGalleryRecord<SelectGallery>
{
    // Data-type templates forwarding to the select

    [Display(Name = "enum")]
    public GalleryCabin Cabin { get; set; }

    [Display(Name = "enum?")]
    public GalleryCabin? OptionalCabin { get; set; }

    [Required]
    [Display(Name = "enum? · [Required]")]
    public GalleryCabin? RequiredCabin { get; set; }

    [Display(Name = "enum · [Display(Description)]", Description = "Shown below the select.")]
    public GalleryCabin DescribedCabin { get; set; }

    [Display(Name = "enum · [Display(GroupName)]")]
    public GalleryFare Fare { get; set; }

    [Display(Name = "bool?")]
    public bool? Insured { get; set; }

    [Display(Name = "[Flags] enum · forwards to the text input")]
    public GalleryExtras Extras { get; set; }

    // Field configuration

    [Display(Name = "enum · this display name is replaced by the field Title")]
    public GalleryCabin TitledCabin { get; set; }

    [Display(Name = "enum · field Description")]
    public GalleryCabin FieldDescribedCabin { get; set; }

    [Editable(false)]
    [Display(Name = "enum · [Editable(false)]")]
    public GalleryCabin ReadOnlyCabin { get; set; }

    [Editable(false)]
    [Display(Name = "bool? · [Editable(false)]")]
    public bool? ReadOnlyInsured { get; set; }

    // UseEditor<SelectEditor> settings

    [Display(Name = "enum? · EmptyChoiceText")]
    public GalleryCabin? AnyCabin { get; set; }

    [Required]
    [Display(Name = "string · [Required] · UseItems")]
    public string? Seat { get; set; }

    [Display(Name = "string? · UseItems with groups, a disabled group and a disabled item")]
    public string? Airport { get; set; }

    [Display(Name = "enum · ClassNames.Control")]
    public GalleryCabin StyledCabin { get; set; }

    public static SelectGallery CreateSample() =>
        new()
        {
            Cabin = GalleryCabin.PremiumEconomy,
            OptionalCabin = GalleryCabin.Business,
            RequiredCabin = GalleryCabin.Economy,
            DescribedCabin = GalleryCabin.First,
            Fare = GalleryFare.Flex,
            Insured = true,
            Extras = GalleryExtras.Meals | GalleryExtras.Lounge,
            TitledCabin = GalleryCabin.Business,
            FieldDescribedCabin = GalleryCabin.Economy,
            ReadOnlyCabin = GalleryCabin.First,
            ReadOnlyInsured = false,
            AnyCabin = null,
            Seat = "window",
            Airport = "LIS",
            StyledCabin = GalleryCabin.Economy,
        };
}
