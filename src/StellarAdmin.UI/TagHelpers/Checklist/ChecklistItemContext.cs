namespace StellarAdmin.UI;

// The parts an item's children rendered, so the item can reject an attribute that a child part
// also sets.
internal sealed class ChecklistItemContext
{
    public bool HasDescription { get; set; }

    public bool HasStatus { get; set; }

    public bool HasTitle { get; set; }
}
