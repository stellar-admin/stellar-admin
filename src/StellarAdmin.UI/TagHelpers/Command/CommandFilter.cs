namespace StellarAdmin.UI;

/// <summary>
///     Specifies how a command menu filters its items as the user types.
/// </summary>
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

internal static class CommandFilterExtensions
{
    extension(CommandFilter filter)
    {
        public string GetDataAttributeText() =>
            filter switch
            {
                CommandFilter.Client => "client",
                CommandFilter.None => "none",
                _ => string.Empty,
            };
    }
}
