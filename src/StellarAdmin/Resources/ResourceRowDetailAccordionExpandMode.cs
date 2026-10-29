namespace StellarAdmin.Resources;

/// <summary>
///     Specifies how many of a row's detail panels can be open at once in an accordion.
/// </summary>
public enum ResourceRowDetailAccordionExpandMode
{
    /// <summary>
    ///     Opening a panel closes the one that was open.
    /// </summary>
    Single,

    /// <summary>
    ///     Any number of panels can be open.
    /// </summary>
    Multiple,
}
