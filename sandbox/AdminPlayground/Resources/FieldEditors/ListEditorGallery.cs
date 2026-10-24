using System.ComponentModel.DataAnnotations;

namespace AdminPlayground.Resources.FieldEditors;

// Each property is one list editor scenario, and its display name describes the scenario.
public sealed class ListEditorGallery
    : FieldEditorGalleryRecord,
        IFieldEditorGalleryRecord<ListEditorGallery>
{
    [Display(Name = "List<string>")]
    public List<string> Highlights { get; set; } = [];

    [Display(Name = "int[]")]
    public int[] DepartureDays { get; set; } = [];

    [Length(1, 4)]
    [Display(Name = "List<string> · [Length(1, 4)]")]
    public List<string> Themes { get; set; } = [];

    [Display(Name = "List<string> · this display name is replaced by the field Title")]
    public List<string> TitledHighlights { get; set; } = [];

    [Display(Name = "List<string> · [Display(Description)]", Description = "Shown below the list.")]
    public List<string> DescribedHighlights { get; set; } = [];

    [Editable(false)]
    [Display(Name = "List<string> · [Editable(false)]")]
    public List<string> ReadOnlyHighlights { get; set; } = [];

    [Display(Name = "List<string> · MinItems 1 · MaxItems 8 · Sortable · Numbered · MaxLength 120")]
    public List<string> Itinerary { get; set; } = [];

    [Display(Name = "List<string> · Unique · Casing Upper · Pattern")]
    public List<string> ArrivalAirports { get; set; } = [];

    [Display(Name = "List<int> · Min 1 · Max 31 · Unique")]
    public List<int> MonthlyDepartures { get; set; } = [];

    [Display(Name = "List<decimal> · Prefix € · Step 0.01")]
    public List<decimal> AddOnPrices { get; set; } = [];

    [Display(Name = "List<int> · Suffix kg · Align Start")]
    public List<int> BagWeights { get; set; } = [];

    [Display(Name = "List<string> · AddText · EmptyText · Placeholder")]
    public List<string> PackingList { get; set; } = [];

    public static ListEditorGallery CreateSample() =>
        new()
        {
            Highlights = ["Tram 28 through Alfama", "Pastéis de Belém"],
            DepartureDays = [4, 11, 18],
            Themes = ["Food", "History"],
            TitledHighlights = ["Fado night"],
            DescribedHighlights = ["Sunset at Miradouro da Graça"],
            ReadOnlyHighlights = ["Jerónimos Monastery", "Belém Tower"],
            Itinerary = ["Arrive in Lisbon", "Sintra palaces", "Day at Cascais"],
            ArrivalAirports = ["LIS", "OPO"],
            MonthlyDepartures = [1, 15],
            AddOnPrices = [35m, 89.5m],
            BagWeights = [8, 23],
        };
}
