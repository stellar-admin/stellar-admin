using System.ComponentModel.DataAnnotations;

namespace DashboardPlayground.Resources.FieldEditors;

// Each property is one multi-select lookup scenario, and its display name describes the scenario.
public sealed class MultiLookupGallery
    : FieldEditorGalleryRecord,
        IFieldEditorGalleryRecord<MultiLookupGallery>
{
    // Layouts

    [Display(Name = "List<string> · Chips (default) · UseCode")]
    public List<string> Destinations { get; set; } = [];

    [Display(Name = "string[] · Chips · UseAvatar")]
    public string[] Guides { get; set; } = [];

    [Display(Name = "List<string> · Layout List · UseCode with UseDescription")]
    public List<string> ListDestinations { get; set; } = [];

    [Display(Name = "string[] · Layout List · UseAvatar with UseDescription")]
    public string[] ListGuides { get; set; } = [];

    [Display(Name = "List<string> · Layout Summary · UseCode")]
    public List<string> SummaryDestinations { get; set; } = [];

    [Display(Name = "string[] · Layout Summary · UseAvatar")]
    public string[] SummaryGuides { get; set; } = [];

    // Field configuration

    [MinLength(1)]
    [Display(Name = "List<string> · [MinLength(1)]")]
    public List<string> RequiredDestinations { get; set; } = [];

    [MaxLength(3)]
    [Display(
        Name = "string[] · [MaxLength(3)]",
        Description = "Up to 3 guides. Server validation rejects more."
    )]
    public string[] LimitedGuides { get; set; } = [];

    [Display(Name = "List<string> · values the source does not have")]
    public List<string> UnknownDestinations { get; set; } = [];

    [Display(Name = "List<string> · editor EmptyText · sheet Title")]
    public List<string> LabelledDestinations { get; set; } = [];

    [Display(Name = "string[] · editor ShowMedia false")]
    public string[] MediaHiddenGuides { get; set; } = [];

    [Display(Name = "List<string> · ClassNames.Control · ClassNames.Media")]
    public List<string> StyledDestinations { get; set; } = [];

    // Read-only

    [Editable(false)]
    [Display(Name = "List<string> · [Editable(false)] · Chips")]
    public List<string> ReadOnlyDestinations { get; set; } = [];

    [Editable(false)]
    [Display(Name = "string[] · [Editable(false)] · List")]
    public string[] ReadOnlyListGuides { get; set; } = [];

    [Editable(false)]
    [Display(Name = "string[] · [Editable(false)] · Summary shows a list")]
    public string[] ReadOnlySummaryGuides { get; set; } = [];

    [Editable(false)]
    [Display(Name = "List<string> · [Editable(false)] · empty")]
    public List<string> ReadOnlyEmptyDestinations { get; set; } = [];

    public static MultiLookupGallery CreateSample() =>
        new()
        {
            Destinations = ["LIS", "MAD", "BCN"],
            Guides = ["ana-ribeiro", "camila-torres"],
            ListDestinations = ["LIS", "CPT", "SYD"],
            ListGuides = ["hiro-tanaka", "ingrid-larsen"],
            SummaryDestinations = ["LIS", "MAD", "BCN", "FCO", "ATH"],
            SummaryGuides = ["ana-ribeiro", "bruno-costa", "camila-torres", "diego-navarro"],
            RequiredDestinations = ["JFK"],
            LimitedGuides = ["elena-marchetti", "lucas-moreau"],
            UnknownDestinations = ["LIS", "XXX"],
            LabelledDestinations = [],
            MediaHiddenGuides = ["grace-mwangi", "sipho-dlamini"],
            StyledDestinations = ["NBO", "JNB"],
            ReadOnlyDestinations = ["CDG", "AMS"],
            ReadOnlyListGuides = ["nikos-pappas", "olivia-brown"],
            ReadOnlySummaryGuides = ["maya-cohen", "pedro-alves", "kavya-nair"],
            ReadOnlyEmptyDestinations = [],
        };
}
