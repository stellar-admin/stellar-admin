using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DocsSamples.Pages.Field;

public class Index : PageModel
{
    public IndexModel Model { get; set; } =
        new IndexModel
        {
            CheckboxImplicit = new CheckboxImplicitModel { SyncMapWithItinerary = true },
            FieldGroupImplicit = new FieldGroupImplicitModel
            {
                EnableBookingPushNotifications = true,
            },
            FieldsetImplicit = new FieldsetImplicitModel(),
            Implicit = new ImplicitModel(),
            InputImplicit = new InputImplicitModel(),
            RadioImplicit = new RadioImplicitModel { InsuranceCover = "annual" },
            SelectImplicit = new SelectImplicitModel { TravelStyle = "beach" },
            TextareaImplicit = new TextareaImplicitModel(),
        };

    public void OnGet() { }
}

public class IndexModel
{
    public CheckboxImplicitModel CheckboxImplicit { get; set; }

    public FieldGroupImplicitModel FieldGroupImplicit { get; set; }

    public FieldsetImplicitModel FieldsetImplicit { get; set; }

    public ImplicitModel Implicit { get; set; }

    public InputImplicitModel InputImplicit { get; set; }

    public RadioImplicitModel RadioImplicit { get; set; }

    public SelectImplicitModel SelectImplicit { get; set; }

    public TextareaImplicitModel TextareaImplicit { get; set; }
}

public class CheckboxImplicitModel
{
    [Display(Name = "Landmarks and attractions")]
    public bool ShowLandmarks { get; set; }

    [Display(Name = "Restaurants and bars")]
    public bool ShowRestaurants { get; set; }

    [Display(Name = "Local tour guides")]
    public bool ShowTourGuides { get; set; }

    [Display(Name = "Transit stations")]
    public bool ShowTransitStations { get; set; }

    [Display(
        Name = "Sync map with the itinerary",
        Description = "Places from the itinerary are shown on the map automatically. Changes you make here are visible to all travellers on the booking."
    )]
    public bool SyncMapWithItinerary { get; set; }
}

public class FieldGroupImplicitModel
{
    [Display(Name = "Push notifications")]
    public bool EnableBookingPushNotifications { get; set; }

    [Display(Name = "Email notifications")]
    public bool EnableFlightEmailNotifications { get; set; }

    [Display(Name = "Push notifications")]
    public bool EnableFlightPushNotifications { get; set; }
}

public class FieldsetImplicitModel
{
    [Display(Name = "Street Address", Prompt = "12 Rue de la Kasbah")]
    public string? Street { get; set; }

    [Display(Name = "City", Prompt = "Marrakech")]
    public string? City { get; set; }

    [Display(Name = "Postal Code", Prompt = "40000")]
    public string? PostalCode { get; set; }
}

public class ImplicitModel
{
    [Display(
        Name = "Card Number",
        Description = "Enter your 16-digit card number",
        Prompt = "1234 5678 9012 3456"
    )]
    public string? CardNumber { get; set; }

    [Display(Name = "Comments", Prompt = "Add any additional comments")]
    public string? Comments { get; set; }

    [Display(Name = "CVV")]
    public string? Cvv { get; set; }

    [Display(Name = "Same as shipping address")]
    public bool IsBillingAddressSame { get; set; }

    public string? Month { get; set; }

    [Display(Name = "Name on Card", Prompt = "Ibn Battuta")]
    public string? Name { get; set; }

    public string? Year { get; set; }
}

public class InputImplicitModel
{
    [Display(
        Name = "Email",
        Description = "Your email address",
        Prompt = "e.g. ibn.battuta@rihlah.travel"
    )]
    [DataType(DataType.EmailAddress)]
    public string? Email { get; set; }

    [Display(
        Name = "Password",
        Description = "Must be at least 8 characters long.",
        Prompt = "••••••••"
    )]
    [DataType(DataType.Password)]
    public string? Password { get; set; }
}

public class RadioImplicitModel
{
    public string? InsuranceCover { get; set; }
}

public class SelectImplicitModel
{
    [Display(Name = "Travel style", Description = "Select the type of trip you take most often.")]
    public string? TravelStyle { get; set; }
}

public class TextareaImplicitModel
{
    [Display(
        Name = "Feedback",
        Description = "Share your thoughts about our service.",
        Prompt = "Your feedback helps us improve..."
    )]
    public string? Feedback { get; set; }
}
