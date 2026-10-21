namespace StellarAdmin.UI;

/// <summary>
///     Specifies the size of the tab that names the environment on a <c>&lt;sa-environment-stripe&gt;</c>.
/// </summary>
public enum EnvironmentStripeTabSize
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

internal static class EnvironmentStripeTabSizeExtensions
{
    extension(EnvironmentStripeTabSize size)
    {
        public string GetDataAttributeText() =>
            size switch
            {
                EnvironmentStripeTabSize.Small => "sm",
                EnvironmentStripeTabSize.Medium => "md",
                EnvironmentStripeTabSize.Large => "lg",
                EnvironmentStripeTabSize.ExtraLarge => "xl",
                _ => string.Empty,
            };
    }
}
