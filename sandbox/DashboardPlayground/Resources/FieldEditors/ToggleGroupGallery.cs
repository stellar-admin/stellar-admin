using System.ComponentModel.DataAnnotations;

namespace DashboardPlayground.Resources.FieldEditors;

// Each property is one toggle group scenario, and its display name describes the scenario.
public sealed class ToggleGroupGallery
    : FieldEditorGalleryRecord,
        IFieldEditorGalleryRecord<ToggleGroupGallery>
{
    [Display(Name = "List<enum> · Chips")]
    public List<GalleryCabin> Cabins { get; set; } = [];

    [Display(Name = "string[] · UseItems · Chips")]
    public string[] Amenities { get; set; } = [];

    [Display(Name = "enum · Chips")]
    public GalleryCabin Cabin { get; set; }

    [Display(Name = "string[] · UseItems · Joined")]
    public string[] Days { get; set; } = [];

    [Display(Name = "enum? · Joined")]
    public GalleryCabin? OptionalCabin { get; set; }

    [Display(Name = "string[] · UseItems · Buttons")]
    public string[] Meals { get; set; } = [];

    [Display(Name = "bool? · Buttons")]
    public bool? Insured { get; set; }

    [Display(Name = "List<enum> · this display name is replaced by the field Title")]
    public List<GalleryCabin> TitledCabins { get; set; } = [];

    [Display(
        Name = "List<enum> · [Display(Description)]",
        Description = "Shown below the toggles."
    )]
    public List<GalleryCabin> DescribedCabins { get; set; } = [];

    [Editable(false)]
    [Display(Name = "List<enum> · [Editable(false)]")]
    public List<GalleryCabin> ReadOnlyCabins { get; set; } = [];

    [Display(Name = "List<enum> · ClassNames.Control")]
    public List<GalleryCabin> StyledCabins { get; set; } = [];

    [Display(Name = "string[] · UseItems with icons · Chips")]
    public string[] Transports { get; set; } = [];

    [Display(Name = "string[] · UseItems with avatars · Chips · CheckPlacement = Start")]
    public string[] Guides { get; set; } = [];

    [Display(Name = "string[] · UseItems with codes · Chips · CheckPlacement = End")]
    public string[] AirportCodes { get; set; } = [];

    [Display(Name = "string · UseItems with images · Joined")]
    public string? City { get; set; }

    [Display(Name = "string[] · UseItems with icons · Buttons")]
    public string[] ButtonTransports { get; set; } = [];

    public static ToggleGroupGallery CreateSample() =>
        new()
        {
            Cabins = [GalleryCabin.Economy, GalleryCabin.Business],
            Amenities = ["wifi", "lounge", "baggage"],
            Cabin = GalleryCabin.PremiumEconomy,
            Days = ["mon", "wed", "fri"],
            OptionalCabin = null,
            Meals = ["breakfast"],
            Insured = true,
            TitledCabins = [GalleryCabin.First],
            DescribedCabins = [GalleryCabin.Economy],
            ReadOnlyCabins = [GalleryCabin.Business],
            StyledCabins = [],
            Transports = ["flight", "ferry"],
            Guides = ["ana", "yuki"],
            AirportCodes = ["LIS", "KIX"],
            City = "cape-town",
            ButtonTransports = ["train"],
        };
}
