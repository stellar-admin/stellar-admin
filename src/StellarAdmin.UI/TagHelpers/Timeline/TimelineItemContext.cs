namespace StellarAdmin.UI;

// The parts an item's children rendered, so the item can reject an attribute that a child part
// also sets, and leave out its own marker when a child supplies media.
internal sealed class TimelineItemContext
{
    public bool HasDescription { get; set; }

    public bool HasMedia { get; set; }

    public bool HasTime { get; set; }

    public bool HasTitle { get; set; }
}
