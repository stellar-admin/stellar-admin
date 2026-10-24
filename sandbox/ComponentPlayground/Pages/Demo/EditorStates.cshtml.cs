using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ComponentPlayground.Pages.Demo;

// The current normal, disabled and read-only states of every UI library editor, side by side,
// for the read-only fields plan (docs/plans/archive/read-only-fields.md).
public class EditorStates : PageModel
{
    public EditorStatesModel Normal { get; set; } = new EditorStatesModel();

    public EditorStatesModel Disabled { get; set; } = new EditorStatesModel();

    public EditorStatesModel ReadOnly { get; set; } = new EditorStatesModel();

    public void OnGet() { }
}

public class EditorStatesModel
{
    [Display(Name = "Highlights")]
    public List<string> Highlights { get; set; } =
    ["Tasting at a family-run tasca in Alfama", "Pastéis de nata in Belém"];

    [Display(Name = "Local names")]
    public Dictionary<string, string> LocalNames { get; set; } =
        new() { ["pt"] = "Passeio por Lisboa", ["es"] = "Ruta por Lisboa" };
}
