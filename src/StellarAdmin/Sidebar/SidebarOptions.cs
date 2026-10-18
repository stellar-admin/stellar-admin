namespace StellarAdmin.Sidebar;

internal sealed class SidebarOptions
{
    public Dictionary<string, SidebarGroupOptions> Groups { get; } = new(StringComparer.Ordinal);

    public bool SortGroupsByLabel { get; set; }

    public bool SortItemsByLabel { get; set; }
}
