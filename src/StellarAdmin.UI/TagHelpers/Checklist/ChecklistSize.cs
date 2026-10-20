namespace StellarAdmin.UI;

/// <summary>
///     Specifies the size of a checklist, which sets the size of its status icons and the spacing
///     between its rows.
/// </summary>
public enum ChecklistSize
{
    /// <summary>
    ///     The standard size.
    /// </summary>
    Default,

    /// <summary>
    ///     A compact size, for a checklist in a card or nested in another checklist.
    /// </summary>
    Small,
}

internal static class ChecklistSizeExtensions
{
    extension(ChecklistSize size)
    {
        public string GetDataAttributeText()
        {
            return size switch
            {
                ChecklistSize.Default => "default",
                ChecklistSize.Small => "sm",
                _ => string.Empty,
            };
        }
    }
}
