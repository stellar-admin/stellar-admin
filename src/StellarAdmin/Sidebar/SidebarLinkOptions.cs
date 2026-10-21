namespace StellarAdmin.Sidebar;

internal sealed class SidebarLinkOptions(string label, SidebarLinkTarget target)
{
    public List<object> AuthorizationMetadata { get; } = [];

    public string? Group { get; set; }

    public string? Icon { get; set; }

    public string Label { get; } = label;

    public bool OpenInNewTab { get; set; }

    public int? Order { get; set; }

    public int Position { get; set; }

    public SidebarLinkTarget Target { get; } = target;
}
