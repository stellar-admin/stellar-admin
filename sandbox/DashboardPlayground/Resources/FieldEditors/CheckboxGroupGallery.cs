using System.ComponentModel.DataAnnotations;

namespace DashboardPlayground.Resources.FieldEditors;

// Each property is one checkbox group scenario, and its display name describes the scenario.
public sealed class CheckboxGroupGallery
    : FieldEditorGalleryRecord,
        IFieldEditorGalleryRecord<CheckboxGroupGallery>
{
    [Display(Name = "List<enum>")]
    public List<GalleryCabin> Cabins { get; set; } = [];

    [MinLength(1)]
    [Display(Name = "List<enum> · [MinLength(1)]")]
    public List<GalleryCabin> RequiredCabins { get; set; } = [];

    [Display(Name = "string[] · UseItems")]
    public string[] Seats { get; set; } = [];

    [Display(Name = "List<enum> · this display name is replaced by the field Title")]
    public List<GalleryCabin> TitledCabins { get; set; } = [];

    [Display(
        Name = "List<enum> · [Display(Description)]",
        Description = "Shown above the choices."
    )]
    public List<GalleryCabin> AttributeDescribedCabins { get; set; } = [];

    [Display(Name = "List<enum> · field Description")]
    public List<GalleryCabin> DescribedCabins { get; set; } = [];

    [Editable(false)]
    [Display(Name = "List<enum> · [Editable(false)]")]
    public List<GalleryCabin> ReadOnlyCabins { get; set; } = [];

    [Display(Name = "List<enum> · ClassNames.Control")]
    public List<GalleryCabin> StyledCabins { get; set; } = [];

    [Display(Name = "List<enum> · Appearance = Cards")]
    public List<GalleryCabin> CardCabins { get; set; } = [];

    [Display(Name = "string[] · Columns(2)")]
    public string[] Amenities { get; set; } = [];

    [Display(Name = "string[] · Columns(Small(2), Large(3)) · Flow = Across")]
    public string[] AcrossAmenities { get; set; } = [];

    [Display(Name = "List<enum> · Appearance = Cards · Columns(2)")]
    public List<GalleryCabin> CardColumnCabins { get; set; } = [];

    [Display(Name = "string[] · Columns(2) in a half-width field")]
    public string[] NarrowAmenities { get; set; } = [];

    [Display(Name = "string[] · Columns(2) in a half-width field, beside it")]
    public string[] NarrowAmenitiesBeside { get; set; } = [];

    [Display(Name = "string[] · UseItems with icons")]
    public string[] Transports { get; set; } = [];

    [Display(Name = "string[] · UseItems with codes and descriptions")]
    public string[] AirportCodes { get; set; } = [];

    [Display(Name = "string[] · UseItems with avatars · Cards")]
    public string[] Guides { get; set; } = [];

    [Display(Name = "string[] · UseItems with images · Cards · Columns(2)")]
    public string[] Cities { get; set; } = [];

    public static CheckboxGroupGallery CreateSample() =>
        new()
        {
            Cabins = [GalleryCabin.Economy, GalleryCabin.Business],
            RequiredCabins = [GalleryCabin.First],
            Seats = ["aisle", "window"],
            TitledCabins = [GalleryCabin.PremiumEconomy],
            AttributeDescribedCabins = [GalleryCabin.First],
            DescribedCabins = [],
            ReadOnlyCabins = [GalleryCabin.Business],
            StyledCabins = [GalleryCabin.Economy],
            CardCabins = [GalleryCabin.Business],
            Amenities = ["wifi", "seat"],
            AcrossAmenities = ["lounge", "transfer"],
            CardColumnCabins = [GalleryCabin.PremiumEconomy],
            NarrowAmenities = ["wifi"],
            NarrowAmenitiesBeside = ["insurance"],
            Transports = ["flight", "ferry"],
            AirportCodes = ["CPT"],
            Guides = ["ana", "lena"],
            Cities = ["lisbon", "kyoto"],
        };
}
