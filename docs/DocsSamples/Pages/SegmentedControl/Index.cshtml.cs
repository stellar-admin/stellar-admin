using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DocsSamples.Pages.SegmentedControl;

public class Index : PageModel
{
    [BindProperty]
    public BookingModel Booking { get; set; } = new();

    public string? Result { get; set; }

    public BookingModel Validation { get; set; } = new() { Cabin = null };

    public void OnGet()
    {
        ModelState.AddModelError("Validation.Cabin", "Choose a cabin class.");
    }

    public void OnPost()
    {
        if (ModelState.IsValid)
        {
            Result = $"Cabin saved: {Booking.Cabin}";
        }
    }

    public class BookingModel
    {
        [Display(Name = "Cabin class", Description = "Choose a cabin for your next flight.")]
        [Required(ErrorMessage = "Choose a cabin class.")]
        [EnumDataType(typeof(CabinClass))]
        public CabinClass? Cabin { get; set; } = CabinClass.Economy;
    }

    public enum CabinClass
    {
        Economy,
        Business,
        First,
    }
}
