namespace StellarAdmin.UI;

// The anchored-* class that places a popover, tooltip or dropdown menu panel against its trigger.
// The classes are in Client/css/structure.css.
internal static class AnchorPlacement
{
    public static string GetClassName(string side, string align) => $"anchored-{side}-{align}";
}
