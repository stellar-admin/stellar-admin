using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DocsSamples.Pages.Questionnaire;

public class Index : PageModel
{
    public IndexModel Model { get; set; } = new IndexModel();

    public void OnGet()
    {
        ModelState.AddModelError(
            "Model.Validation.CabinClass",
            "Choose a cabin class to continue."
        );
    }

    public class IndexModel
    {
        public CardModel Card { get; set; } = new CardModel();

        public FreeformModel Freeform { get; set; } = new FreeformModel();

        public IntroModel Intro { get; set; } = new IntroModel();

        public LongFormModel LongForm { get; set; } = new LongFormModel();

        public MultipleModel Multiple { get; set; } = new MultipleModel();

        public ShortcutsModel Shortcuts { get; set; } = new ShortcutsModel();

        public StepsModel Steps { get; set; } = new StepsModel();

        public ValidationModel Validation { get; set; } = new ValidationModel();
    }

    public class CardModel
    {
        public string? SeatPreference { get; set; } = "window";
    }

    public class FreeformModel
    {
        public string? Destination { get; set; }

        public string? OtherDestination { get; set; }
    }

    public class IntroModel
    {
        public string? TravelStyle { get; set; } = "one-stop";
    }

    public class LongFormModel
    {
        public string? Accommodation { get; set; } = "boutique";

        public string? Budget { get; set; }

        public string[] Interests { get; set; } = ["food"];
    }

    public class MultipleModel
    {
        public string[] Extras { get; set; } = ["transfers", "insurance"];
    }

    public class ShortcutsModel
    {
        public string? Departure { get; set; }
    }

    public class StepsModel
    {
        public string? Pace { get; set; } = "relaxed";
    }

    public class ValidationModel
    {
        [Required(ErrorMessage = "Choose a cabin class to continue.")]
        public string? CabinClass { get; set; }
    }
}
