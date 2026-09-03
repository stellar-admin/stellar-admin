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
        ModelState.AddModelError(
            "Model.Validation.OtherOccasion",
            "Tell us a little more about the occasion."
        );
    }

    public class IndexModel
    {
        public CardModel Card { get; set; } = new CardModel();

        public CustomProgressModel CustomProgress { get; set; } = new CustomProgressModel();

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

    public class CustomProgressModel
    {
        public int Current { get; set; } = 2;

        public int Total { get; set; } = 4;

        public string? Travellers { get; set; } = "couple";
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

        public string? OtherAccommodation { get; set; }
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

        public string? Occasion { get; set; }

        [MinLength(15, ErrorMessage = "Tell us a little more about the occasion.")]
        public string? OtherOccasion { get; set; } = "eclipse";
    }
}
