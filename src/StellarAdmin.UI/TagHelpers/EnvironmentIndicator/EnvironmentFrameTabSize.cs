namespace StellarAdmin.UI;

/// <summary>
///     Specifies the size of the tab that names the environment on a <c>&lt;sa-environment-frame&gt;</c>.
/// </summary>
public enum EnvironmentFrameTabSize
{
    /// <summary>
    ///     A small tab.
    /// </summary>
    Small,

    /// <summary>
    ///     A medium tab.
    /// </summary>
    Medium,

    /// <summary>
    ///     A large tab.
    /// </summary>
    Large,

    /// <summary>
    ///     An extra large tab.
    /// </summary>
    ExtraLarge,
}

internal static class EnvironmentFrameTabSizeExtensions
{
    extension(EnvironmentFrameTabSize size)
    {
        public string GetDataAttributeText() =>
            size switch
            {
                EnvironmentFrameTabSize.Small => "sm",
                EnvironmentFrameTabSize.Medium => "md",
                EnvironmentFrameTabSize.Large => "lg",
                EnvironmentFrameTabSize.ExtraLarge => "xl",
                _ => string.Empty,
            };
    }
}
