using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using StellarAdmin.UI.Icons;

namespace DocsSamples.Pages.IconPicker;

// The demos' icons endpoint: every registered icon, in the payload that icons-url documents.
public class Index : PageModel
{
    public IndexModel Model { get; set; } = new IndexModel();

    public void OnGet() { }

    public IActionResult OnGetIcons([FromServices] IOptions<IconOptions> iconOptions)
    {
        var icons = iconOptions.Value;
        var payload = icons
            .GetIconNames()
            .Order(StringComparer.Ordinal)
            .Select(name =>
            {
                icons.TryGetIcon(name, out var icon);

                return new
                {
                    name,
                    svg = ToSvg(icon!),
                    keywords = icon!.Keywords,
                };
            });

        return new JsonResult(new { icons = payload });
    }

    private static string ToSvg(IconDefinition icon)
    {
        var svg = new StringBuilder("<svg");
        foreach (var (name, value) in icon.Attributes)
        {
            svg.Append($" {name}=\"{WebUtility.HtmlEncode(value)}\"");
        }

        svg.Append('>');
        foreach (var shape in icon.Shapes)
        {
            svg.Append($"<{shape.Name}");
            foreach (var (name, value) in shape.Attributes)
            {
                svg.Append($" {name}=\"{WebUtility.HtmlEncode(value)}\"");
            }

            svg.Append("/>");
        }

        return svg.Append("</svg>").ToString();
    }

    public class IndexModel
    {
        public ModelBindingModel ModelBinding { get; set; } = new ModelBindingModel();
    }

    public class ModelBindingModel
    {
        [Required]
        [Display(Name = "Tour type icon", Description = "Shown on the tour card.")]
        public string Icon { get; set; } = "mountain";
    }
}
