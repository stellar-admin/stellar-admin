namespace StellarAdmin.Pro.Identity.Options;

/// <summary>
///     The configured options for a single sidebar item.
/// </summary>
public class SidebarItemOptions
{
    /// <summary>The item label.</summary>
    /// <remarks>Defaults to the title of the page the item links to.</remarks>
    public string? Label { get; internal set; }

    /// <summary>Whether the item renders in the sidebar.</summary>
    /// <remarks>Cosmetic only. Hiding the item does not disable the screens it links to.</remarks>
    public bool Visible { get; internal set; } = true;
}
