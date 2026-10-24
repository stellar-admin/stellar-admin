using System.ComponentModel.DataAnnotations;

namespace AdminPlayground.Resources.FieldEditors;

public enum GallerySeason
{
    [Display(Name = "Low season")]
    Low,

    [Display(Name = "Shoulder season")]
    Shoulder,

    [Display(Name = "High season")]
    High,
}

// Each property is one dictionary editor scenario, and its display name describes the scenario.
public sealed class DictionaryEditorGallery
    : FieldEditorGalleryRecord,
        IFieldEditorGalleryRecord<DictionaryEditorGallery>
{
    [Display(Name = "Dictionary<string, string>")]
    public Dictionary<string, string> Notes { get; set; } = [];

    [Display(Name = "Dictionary<GallerySeason, decimal>")]
    public Dictionary<GallerySeason, decimal> SeasonalPrices { get; set; } = [];

    [Display(
        Name = "Dictionary<string, string> · this display name is replaced by the field Title"
    )]
    public Dictionary<string, string> TitledNotes { get; set; } = [];

    [Editable(false)]
    [Display(Name = "Dictionary<string, string> · [Editable(false)]")]
    public Dictionary<string, string> ReadOnlyNotes { get; set; } = [];

    [Display(Name = "Dictionary<string, string> · Disabled()")]
    public Dictionary<string, string> DisabledNotes { get; set; } = [];

    [Display(Name = "Dictionary<string, string> · key Pattern · key Casing Lower · labels")]
    public Dictionary<string, string> LocalNames { get; set; } = [];

    [Display(Name = "Dictionary<string, int> · value Min 0 · Max 50 · Suffix kg")]
    public Dictionary<string, int> BaggageAllowance { get; set; } = [];

    [Display(Name = "Dictionary<string, int> · key UseItems · value UseResource<Guide>")]
    public Dictionary<string, int> GuidesByLanguage { get; set; } = [];

    [Display(Name = "Dictionary<GallerySeason, decimal> · MinItems 1 · Numbered · value Prefix €")]
    public Dictionary<GallerySeason, decimal> TourPrices { get; set; } = [];

    public static DictionaryEditorGallery CreateSample() =>
        new()
        {
            Notes = new() { ["Meeting point"] = "Praça do Comércio" },
            SeasonalPrices = new() { [GallerySeason.Low] = 49.5m, [GallerySeason.High] = 69m },
            TitledNotes = new() { ["Dress code"] = "Comfortable shoes" },
            ReadOnlyNotes = new() { ["Operator"] = "Voyager Travel Lisbon" },
            DisabledNotes = new() { ["Operator"] = "Voyager Travel Lisbon" },
            LocalNames = new() { ["pt"] = "Passeio por Lisboa", ["es"] = "Ruta por Lisboa" },
            BaggageAllowance = new() { ["Economy"] = 23, ["Business"] = 32 },
            GuidesByLanguage = new() { ["pt"] = 1 },
            TourPrices = new() { [GallerySeason.Shoulder] = 59m },
        };
}
