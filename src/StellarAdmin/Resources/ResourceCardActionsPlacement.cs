namespace StellarAdmin.Resources;

/// <summary>
///     Specifies where the row actions sit on the cards of a cards data layout.
/// </summary>
public enum ResourceCardActionsPlacement
{
    /// <summary>
    ///     A menu in the card's header, or on the image when the title is over it.
    /// </summary>
    Menu,

    /// <summary>
    ///     A footer with an edit button and a menu with the other actions.
    /// </summary>
    Footer,
}
