using Microsoft.AspNetCore.Mvc.RazorPages;
using StellarAdmin.Icons;

namespace DocsSamples.Pages.Icon;

public class Semantic : PageModel
{
    public IReadOnlyList<PackPreview> Packs { get; } =
    [
        new("Lucide", "preview-lucide-", new LucideIconPack().GetSemanticIconMappings()),
        new(
            "Tabler Outline",
            "preview-tabler-outline-",
            new TablerOutlineIconPack().GetSemanticIconMappings()
        ),
        new(
            "Tabler Filled",
            "preview-tabler-filled-",
            new TablerFilledIconPack().GetSemanticIconMappings()
        ),
        new("Voyager (sample)", "", new VoyagerIconPack().GetSemanticIconMappings()),
    ];

    public IReadOnlyList<SemanticIconRole> Roles { get; } =
        Enum.GetValues<SemanticIconRole>()
            .OrderBy(role => role.ToString(), StringComparer.Ordinal)
            .ToArray();

    public void OnGet() { }

    public record PackPreview(
        string Name,
        string Prefix,
        IReadOnlyDictionary<SemanticIconRole, string> Mappings
    );
}
