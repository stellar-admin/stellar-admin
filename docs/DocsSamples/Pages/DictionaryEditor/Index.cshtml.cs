using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DocsSamples.Pages.DictionaryEditor;

public class Index : PageModel
{
    public static readonly IReadOnlyList<SelectListItem> Languages =
    [
        new("Portuguese", "pt"),
        new("English", "en"),
        new("Spanish", "es"),
        new("French", "fr"),
        new("German", "de"),
        new("Japanese", "ja"),
    ];

    public static readonly IReadOnlyList<SelectListItem> Guides =
    [
        new("Inês Duarte", "ines"),
        new("Tiago Moreira", "tiago"),
        new("Beatriz Lopes", "beatriz"),
        new("Haruka Sato", "haruka"),
    ];

    public IndexModel Model { get; set; } = new IndexModel();

    public void OnGet()
    {
        AddValidationErrors(ModelState, "Model.Validation");
    }

    // The errors a server check would return for a key and for a value
    public static void AddValidationErrors(ModelStateDictionary modelState, string prefix)
    {
        modelState.AddModelError(
            $"{prefix}.BaggageAllowance[1].Value",
            "Business allows no more than 32 kg on this route."
        );
    }

    public class IndexModel
    {
        public LocalNamesModel Intro { get; set; } = new LocalNamesModel();

        public SeasonalPricesModel EnumKeys { get; set; } = new SeasonalPricesModel();

        public GuidesModel Items { get; set; } = new GuidesModel();

        public BaggageModel Numbers { get; set; } = new BaggageModel();

        public BaggageModel Validation { get; set; } =
            new BaggageModel
            {
                BaggageAllowance = new() { ["Economy"] = 23, ["Business"] = 40 },
            };

        public SeasonalPricesModel ReadOnly { get; set; } = new SeasonalPricesModel();
    }

    public class LocalNamesModel
    {
        [Display(Name = "Local names", Description = "The trip's name on localised pages.")]
        public Dictionary<string, string> LocalNames { get; set; } =
            new()
            {
                ["pt"] = "Passeio gastronómico por Lisboa",
                ["es"] = "Ruta gastronómica por Lisboa",
                ["ja"] = "リスボン食べ歩きツアー",
            };
    }

    public class SeasonalPricesModel
    {
        [Display(Name = "Seasonal prices")]
        public Dictionary<Season, decimal> SeasonalPrices { get; set; } =
            new() { [Season.Low] = 49m, [Season.High] = 69m };
    }

    public class GuidesModel
    {
        [Display(Name = "Guides by language", Description = "Who leads the walk in each language.")]
        public Dictionary<string, string> GuidesByLanguage { get; set; } =
            new()
            {
                ["pt"] = "ines",
                ["en"] = "tiago",
                ["ja"] = "haruka",
            };
    }

    public class BaggageModel
    {
        [Display(Name = "Baggage allowance")]
        public Dictionary<string, int> BaggageAllowance { get; set; } =
            new() { ["Economy"] = 23, ["Business"] = 32 };
    }

    public enum Season
    {
        [Display(Name = "Low season")]
        Low,

        [Display(Name = "Shoulder season")]
        Shoulder,

        [Display(Name = "High season")]
        High,

        [Display(Name = "Peak (holidays)")]
        Peak,
    }
}
