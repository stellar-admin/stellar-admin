namespace StellarAdmin.Dashboard.Sidebar;

internal sealed class SidebarLinkOptions(string label, SidebarLinkTarget target)
{
    public List<object> AuthorizationMetadata { get; } = [];

    public string? Group
    {
        get;
        set
        {
            if (value is not null)
            {
                ArgumentException.ThrowIfNullOrWhiteSpace(value);
            }

            field = value;
        }
    }

    public string Label { get; } = label;

    public bool OpenInNewTab { get; set; }

    public int Order { get; set; }

    public SidebarLinkTarget Target { get; } = target;
}
