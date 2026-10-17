using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ComponentPlayground.Pages.Demo;

public class DropdownMenuItems : PageModel
{
    public string? Posted { get; private set; }

    public void OnGet() { }

    public void OnPostChoose(string choice) => Posted = choice;
}
