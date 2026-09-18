using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DocsSamples.Pages.RadioGroup;

public class Index : PageModel
{
    [BindProperty]
    public OrderModel Order { get; set; } = new();

    public string? Result { get; set; }

    public ValidationModel Validation { get; set; } = new();

    public void OnGet()
    {
        ModelState.AddModelError("Validation.DeliveryMethod", "Choose a delivery method.");
    }

    public void OnPost()
    {
        if (ModelState.IsValid)
        {
            Result = $"Saved: {Order.DeliveryMethod}";
        }
    }

    public class OrderModel
    {
        [Display(Name = "Delivery method", Description = "Choose how we deliver your order.")]
        [Required(ErrorMessage = "Choose a delivery method.")]
        public Delivery? DeliveryMethod { get; set; } = Delivery.Standard;
    }

    public class ValidationModel
    {
        [Display(Name = "Delivery method", Description = "Choose how we deliver your order.")]
        [Required(ErrorMessage = "Choose a delivery method.")]
        public Delivery? DeliveryMethod { get; set; }
    }

    public enum Delivery
    {
        Standard,
        Express,
    }
}
