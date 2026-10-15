namespace StellarAdmin.Sidebar;

internal sealed record ResourceSidebarItem(
    string Label,
    string Group,
    string? Icon,
    int Order,
    bool Visible
);
