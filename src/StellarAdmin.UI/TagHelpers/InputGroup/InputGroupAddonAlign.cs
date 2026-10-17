namespace StellarAdmin.UI;

/// <summary>
///     Where an input group add-on is positioned relative to the input.
/// </summary>
public enum InputGroupAddonAlign
{
    /// <summary>Before the input, on the same row.</summary>
    Start,

    /// <summary>After the input, on the same row.</summary>
    End,

    /// <summary>On its own row above the input.</summary>
    Top,

    /// <summary>On its own row below the input.</summary>
    Bottom,
}

internal static class InputGroupAddonAlignExtensions
{
    extension(InputGroupAddonAlign align)
    {
        public string GetDataAttributeText()
        {
            return align switch
            {
                InputGroupAddonAlign.Start => "start",
                InputGroupAddonAlign.End => "end",
                InputGroupAddonAlign.Top => "top",
                InputGroupAddonAlign.Bottom => "bottom",
                _ => string.Empty,
            };
        }
    }
}
