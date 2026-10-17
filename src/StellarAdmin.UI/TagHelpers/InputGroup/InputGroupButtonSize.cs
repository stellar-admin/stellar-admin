namespace StellarAdmin.UI;

/// <summary>
///     Specifies the size of a button in an input group.
/// </summary>
public enum InputGroupButtonSize
{
    /// <summary>
    ///     An extra-small button.
    /// </summary>
    ExtraSmall,

    /// <summary>
    ///     A small button.
    /// </summary>
    Small,

    /// <summary>
    ///     An extra-small square button for a single icon.
    /// </summary>
    IconExtraSmall,

    /// <summary>
    ///     A small square button for a single icon.
    /// </summary>
    IconSmall,
}

internal static class InputGroupButtonSizeExtensions
{
    extension(InputGroupButtonSize size)
    {
        public string GetDataAttributeText()
        {
            return size switch
            {
                InputGroupButtonSize.ExtraSmall => "xs",
                InputGroupButtonSize.Small => "sm",
                InputGroupButtonSize.IconExtraSmall => "icon-xs",
                InputGroupButtonSize.IconSmall => "icon-sm",
                _ => string.Empty,
            };
        }
    }
}
