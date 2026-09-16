using StellarAdmin.Dashboard.Identity.Options;

namespace StellarAdmin.Dashboard.Identity.Builders;

/// <summary>
///     Configures a single sidebar item.
/// </summary>
public class SidebarItemBuilder
{
    private readonly SidebarItemOptions _options;

    /// <summary>Sets the item label.</summary>
    /// <remarks>Defaults to the title of the page the item links to.</remarks>
    public string? Label
    {
        get => _options.Label;
        set => _options.Label = value;
    }

    /// <summary>Sets whether the item renders in the sidebar.</summary>
    /// <remarks>Cosmetic only. Hiding the item does not disable the screens it links to.</remarks>
    public bool Visible
    {
        get => _options.Visible;
        set => _options.Visible = value;
    }

    public SidebarItemBuilder(SidebarItemOptions options)
    {
        _options = options;
    }
}
