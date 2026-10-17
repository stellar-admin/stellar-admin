namespace StellarAdmin.UI;

/// <summary>
///     The spacing between the items of a toggle group.
/// </summary>
public enum ToggleGroupSpacing
{
    /// <summary>The default spacing between items.</summary>
    Default,

    /// <summary>No spacing, which joins the items into a single segmented control.</summary>
    None,

    /// <summary>Small spacing between items.</summary>
    Small,

    /// <summary>Large spacing between items.</summary>
    Large,
}

internal static class ToggleGroupSpacingExtensions
{
    extension(ToggleGroupSpacing spacing)
    {
        // The Tailwind spacing multiplier fed to the --gap variable and data-spacing.
        public int GetSpacingUnits() =>
            spacing switch
            {
                ToggleGroupSpacing.None => 0,
                ToggleGroupSpacing.Small => 1,
                ToggleGroupSpacing.Large => 4,
                _ => 2,
            };
    }
}
