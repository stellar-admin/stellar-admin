using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DocsSamples.Pages.ListEditor;

public class Index : PageModel
{
    public IndexModel Model { get; set; } = new IndexModel();

    public void OnGet()
    {
        AddValidationErrors(ModelState, "Model.Validation");
    }

    // The errors a server check would return: one for an item, one for the whole list
    public static void AddValidationErrors(ModelStateDictionary modelState, string prefix)
    {
        modelState.AddModelError(
            $"{prefix}.Highlights[1]",
            "Pick a highlight that isn't on another Lisbon walk."
        );
        modelState.AddModelError($"{prefix}.Highlights", "Add a highlight about the food.");
    }

    public class IndexModel
    {
        public TripModel Intro { get; set; } = new TripModel();

        public TripModel Numbered { get; set; } = new TripModel();

        public DeparturesModel Numbers { get; set; } = new DeparturesModel();

        public AirportsModel ItemChecks { get; set; } = new AirportsModel();

        public TrailModel Affixes { get; set; } = new TrailModel();

        public PackingModel Empty { get; set; } = new PackingModel();

        public TripModel Validation { get; set; } = new TripModel();

        public TripModel ReadOnly { get; set; } = new TripModel();

        public TripModel Disabled { get; set; } = new TripModel();
    }

    public class TripModel
    {
        [Display(Name = "Highlights", Description = "Shown as bullet points on the trip page.")]
        [Length(1, 8)]
        public List<string> Highlights { get; set; } =
        [
            "Tasting at a family-run tasca in Alfama",
            "Pastéis de nata, warm from the oven in Belém",
            "Sunset at the Miradouro da Senhora do Monte",
        ];
    }

    public class DeparturesModel
    {
        [Display(Name = "Departure days", Description = "Days of the month the walk departs.")]
        public int[] DepartureDays { get; set; } = [5, 12, 19, 26];
    }

    public class AirportsModel
    {
        [Display(Name = "Arrival airports", Description = "Three-letter IATA codes.")]
        [MaxLength(6)]
        public List<string> ArrivalAirports { get; set; } = ["LIS", "OPO"];
    }

    public class TrailModel
    {
        [Display(
            Name = "Daily distances",
            Description = "One stage of the Rota Vicentina per day."
        )]
        public List<decimal> DailyDistances { get; set; } = [14.5m, 18m, 22.5m];
    }

    public class PackingModel
    {
        [Display(
            Name = "Packing list",
            Description = "Sent to travellers a week before departure."
        )]
        public List<string> PackingList { get; set; } = [];
    }
}
