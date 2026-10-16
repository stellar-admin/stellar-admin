using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DocsSamples.Pages.DataGrid;

public class Index : PageModel
{
    public void OnGet()
    {
        AddEditingErrors(ModelState);
    }

    /// <summary>The errors that the validation errors sample shows, as a Save handler would find them.</summary>
    public static void AddEditingErrors(ModelStateDictionary modelState)
    {
        modelState.AddModelError("rows[TRP-4822].Destination", "Enter a destination.");
        modelState.AddModelError("rows[TRP-4822].Amount", "The amount must be more than zero.");
    }
}
