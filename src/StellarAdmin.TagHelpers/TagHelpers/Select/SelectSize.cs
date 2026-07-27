namespace StellarAdmin.TagHelpers;

/// <summary>
///     The size of a <c>&lt;sa-select&gt;</c>.
/// </summary>
public enum SelectSize
{
    /// <summary>The default select size.</summary>
    Default,

    /// <summary>A smaller, more compact select.</summary>
    Small,
}

internal static class SelectSizeExtensions
{
    extension(SelectSize size)
    {
        public string GetDataAttributeText() =>
            size switch
            {
                SelectSize.Default => "default",
                SelectSize.Small => "sm",
                _ => "",
            };
    }
}
