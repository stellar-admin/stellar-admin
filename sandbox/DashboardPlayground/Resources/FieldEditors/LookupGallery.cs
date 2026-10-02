using System.ComponentModel.DataAnnotations;

namespace DashboardPlayground.Resources.FieldEditors;

// Each property is one lookup scenario, and its display name describes the scenario.
public sealed class LookupGallery
    : FieldEditorGalleryRecord,
        IFieldEditorGalleryRecord<LookupGallery>
{
    // UseItems with and without descriptions

    [Display(Name = "string? · UseItems")]
    public string? Airport { get; set; }

    [Required]
    [Display(Name = "string · [Required] · UseItems")]
    public string? RequiredAirport { get; set; }

    [Display(Name = "string? · UseItems with DescribeWith")]
    public string? DescribedItemsAirport { get; set; }

    // Field configuration

    [Display(Name = "string? · this display name is replaced by the field Title")]
    public string? TitledAirport { get; set; }

    [Display(Name = "string? · [Display(Description)]", Description = "Shown below the lookup.")]
    public string? DescribedAirport { get; set; }

    [Display(Name = "string? · field Description")]
    public string? FieldDescribedAirport { get; set; }

    [Editable(false)]
    [Display(Name = "string? · [Editable(false)]")]
    public string? ReadOnlyAirport { get; set; }

    [Display(Name = "string? · a value the source does not have")]
    public string? UnknownAirport { get; set; }

    // UseEditor<LookupEditor> settings

    [Display(Name = "string? · SheetTitle · SearchPlaceholder · EmptyText")]
    public string? LabelledAirport { get; set; }

    [Display(Name = "string? · AllowClear false")]
    public string? UnclearableAirport { get; set; }

    [Display(Name = "string? · MinimumSearchLength 2")]
    public string? SearchedAirport { get; set; }

    [Display(Name = "string? · PageSize 5")]
    public string? PagedAirport { get; set; }

    [Display(Name = "string? · ClassNames.Control")]
    public string? StyledAirport { get; set; }

    public static LookupGallery CreateSample() =>
        new()
        {
            Airport = "LIS",
            RequiredAirport = "JFK",
            DescribedItemsAirport = "NRT",
            TitledAirport = "CPT",
            DescribedAirport = "SYD",
            FieldDescribedAirport = "YVR",
            ReadOnlyAirport = "CDG",
            UnknownAirport = "XXX",
            LabelledAirport = null,
            UnclearableAirport = "MAD",
            SearchedAirport = "SIN",
            PagedAirport = "DXB",
            StyledAirport = "GRU",
        };
}
