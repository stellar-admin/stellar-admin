using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DataGridSpike.Pages;

public class FormsModel : PageModel
{
    [BindProperty]
    public string? Notes { get; set; }

    public void OnGet() { }
}
