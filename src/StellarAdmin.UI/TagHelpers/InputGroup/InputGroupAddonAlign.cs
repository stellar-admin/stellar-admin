namespace StellarAdmin.UI;

/// <summary>
///     Where an input group add-on is positioned relative to the input.
/// </summary>
public enum InputGroupAddonAlign
{
    /// <summary>Aligned to the leading (inline start) edge of the input.</summary>
    InlineStart,

    /// <summary>Aligned to the trailing (inline end) edge of the input.</summary>
    InlineEnd,

    /// <summary>Placed on its own row above the input.</summary>
    BlockStart,

    /// <summary>Placed on its own row below the input.</summary>
    BlockEnd,
}

internal static class InputGroupAddonAlignExtensions
{
    extension(InputGroupAddonAlign align)
    {
        public string GetDataAttributeText()
        {
            return align switch
            {
                InputGroupAddonAlign.InlineStart => "inline-start",
                InputGroupAddonAlign.InlineEnd => "inline-end",
                InputGroupAddonAlign.BlockStart => "block-start",
                InputGroupAddonAlign.BlockEnd => "block-end",
                _ => string.Empty,
            };
        }
    }
}
