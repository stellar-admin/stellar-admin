namespace StellarAdmin.TagHelpers;

/// <summary>
///     The appearance of a <c>&lt;sa-chip-group&gt;</c>.
/// </summary>
public enum ChipGroupAppearance
{
    /// <summary>A wrapping row of chips on the background that contains it.</summary>
    Plain,

    /// <summary>A box styled like an input, with a focus and invalid ring.</summary>
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
