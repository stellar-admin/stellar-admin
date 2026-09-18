using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DocsSamples.Pages.CheckboxGroup;

public class Index : PageModel
{
    [BindProperty]
    public OrderModel Order { get; set; } = new();

    public string? Result { get; set; }

    public ValidationModel Validation { get; set; } = new();

    public void OnGet()
    {
        ModelState.AddModelError("Validation.Extras", "Choose at least one extra.");
    }

    public void OnPost()
    {
        if (ModelState.IsValid)
        {
            Result = $"Saved: {string.Join(", ", Order.Extras)}";
        }
    }

    public class OrderModel
    {
        [Display(Name = "Extras", Description = "Choose extras, or clear every checkbox and save.")]
        [MaxLength(2, ErrorMessage = "Choose at most two extras.")]
        public int[] Extras { get; set; } = [1, 2];
    }

    public class ValidationModel
    {
        [Display(Name = "Extras", Description = "Choose extras for your order.")]
        [MinLength(1, ErrorMessage = "Choose at least one extra.")]
        public int[] Extras { get; set; } = [];
    }

    public enum Delivery
    {
        Standard,
        Express,
    }
}
