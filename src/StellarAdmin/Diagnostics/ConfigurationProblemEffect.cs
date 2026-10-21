namespace StellarAdmin.Diagnostics;

/// <summary>
///     Specifies what the admin panel did about a configuration problem.
/// </summary>
public enum ConfigurationProblemEffect
{
    /// <summary>
    ///     A setting is ignored, and its default is used.
    /// </summary>
    Ignored,

    /// <summary>
    ///     A setting is corrected to a value the admin panel can use.
    /// </summary>
    Adjusted,

    /// <summary>
    ///     A feature, such as a form field or a page, is turned off.
    /// </summary>
    TurnedOff,

    /// <summary>
    ///     The resource is turned off: it leaves the sidebar and its pages return 404.
    /// </summary>
    ResourceOff,
}
