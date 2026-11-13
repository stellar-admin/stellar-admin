namespace StellarAdmin.UI;

/// <summary>
///     Specifies how a command menu filters its items as the user types.
/// </summary>
[GenerateDataAttributeText]
public enum CommandFilter
{
    /// <summary>
    ///     The browser filters and ranks the items.
    /// </summary>
    Client,

    /// <summary>
    ///     The browser doesn't filter the items. The application replaces the list contents itself,
    ///     for example with results from the server.
    /// </summary>
    None,
}
