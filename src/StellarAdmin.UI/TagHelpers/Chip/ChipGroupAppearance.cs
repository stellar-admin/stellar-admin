namespace StellarAdmin.UI;

/// <summary>
///     Specifies how a chip group looks.
/// </summary>
public enum ChipGroupAppearance
{
    /// <summary>
    ///     A wrapping row of chips on the background that contains it.
    /// </summary>
    Plain,

    /// <summary>
    ///     A box styled like an input, with a focus ring and an invalid ring.
    /// </summary>
    Input,
}

internal static class ChipGroupAppearanceExtensions
{
    extension(ChipGroupAppearance appearance)
    {
        public string GetDataAttributeText() =>
            appearance switch
            {
                ChipGroupAppearance.Plain => "plain",
                ChipGroupAppearance.Input => "input",
                _ => string.Empty,
            };
    }
}
