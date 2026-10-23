using System.ComponentModel.DataAnnotations;

namespace AdminPlayground.Resources.FieldEditors;

// Each property is one lookup scenario, and its display name describes the scenario.
public sealed class LookupGallery
    : FieldEditorGalleryRecord,
        IFieldEditorGalleryRecord<LookupGallery>
{
    // UseSource with and without descriptions

    [Display(Name = "string? · UseSource")]
    public string? Airport { get; set; }

    [Required]
    [Display(Name = "string · [Required] · UseSource")]
    public string? RequiredAirport { get; set; }

    [Display(Name = "string? · UseSource with GetDescription")]
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

    // UseLookupSheetEditor settings

    [Display(Name = "string? · sheet Title · SearchPlaceholder · editor EmptyText")]
    public string? LabelledAirport { get; set; }

    [Display(Name = "string? · editor AllowClear false")]
    public string? UnclearableAirport { get; set; }

    [Display(Name = "string? · sheet MinimumSearchLength 2")]
    public string? SearchedAirport { get; set; }

    [Display(Name = "string? · sheet PageSize 5")]
    public string? PagedAirport { get; set; }

    [Display(Name = "string? · ClassNames.Control")]
    public string? StyledAirport { get; set; }

    // Media and layout

    [Display(Name = "string? · GetCode with GetDescription")]
    public string? CodeAirport { get; set; }

    [Display(Name = "string? · GetCode")]
    public string? CodeInputAirport { get; set; }

    [Display(Name = "string? · GetAvatar with GetDescription")]
    public string? AvatarAirport { get; set; }

    [Display(Name = "string? · GetAvatar")]
    public string? AvatarInputAirport { get; set; }

    [Display(Name = "string? · GetImage with GetDescription")]
    public string? ImageAirport { get; set; }

    [Display(Name = "string? · GetIcon · ClassNames.Media")]
    public string? IconInputAirport { get; set; }

    [Display(Name = "string? · GetDescription · editor Layout Input")]
    public string? InputLayoutAirport { get; set; }

    [Display(Name = "string? · GetCode · editor Layout Card")]
    public string? CardLayoutAirport { get; set; }

    [Display(Name = "string? · GetCode · editor ShowMedia false")]
    public string? EditorMediaHiddenAirport { get; set; }

    [Display(Name = "string? · GetCode · sheet ShowMedia false")]
    public string? SheetMediaHiddenAirport { get; set; }

    [Display(Name = "string? · GetCode · EnableCreate")]
    public string? CreatableAirport { get; set; }

    [Editable(false)]
    [Display(Name = "string? · GetCode · [Editable(false)]")]
    public string? ReadOnlyCodeAirport { get; set; }

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
            CodeAirport = "AMS",
            CodeInputAirport = "BCN",
            AvatarAirport = "HKG",
            AvatarInputAirport = "IST",
            ImageAirport = "LIS",
            IconInputAirport = "JNB",
            InputLayoutAirport = "KEF",
            CardLayoutAirport = "LHR",
            EditorMediaHiddenAirport = "MEX",
            SheetMediaHiddenAirport = "NBO",
            CreatableAirport = null,
            ReadOnlyCodeAirport = "ZRH",
        };
}

// The media of the lookup gallery's airports
internal enum AirportMedia
{
    Code,
    Avatar,
    Image,
    Icon,
}
