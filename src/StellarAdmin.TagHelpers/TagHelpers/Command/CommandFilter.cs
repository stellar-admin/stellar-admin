namespace StellarAdmin.TagHelpers;

/// <summary>
///     How a command menu narrows its items as the user types.
/// </summary>
public enum CommandFilter
{
    /// <summary>
    ///     Items are filtered and ranked in the browser.
    /// </summary>
    Client,

    /// <summary>
    ///     Items are never filtered in the browser; the application replaces the list contents
    ///     itself, for example with server-rendered results.
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
