namespace StellarAdmin.UI;

/// <summary>
///     Specifies the space between the items of a toggle group.
/// </summary>
public enum ToggleGroupSpacing
{
    /// <summary>
    ///     The standard space.
    /// </summary>
    Default,

    /// <summary>
    ///     No space, which joins the items into one control.
    /// </summary>
    None,

    /// <summary>
    ///     A small space.
    /// </summary>
    Small,

    /// <summary>
    ///     A large space.
    /// </summary>
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
