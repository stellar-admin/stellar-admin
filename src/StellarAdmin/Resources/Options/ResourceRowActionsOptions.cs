namespace StellarAdmin.Resources.Options;

/// <summary>
///     Stores which of the resource's row actions a table shows.
/// </summary>
/// <remarks>
///     A table shows an action only when the resource also allows it.
/// </remarks>
public sealed class ResourceRowActionsOptions
{
    /// <summary>
    ///     Gets a value that indicates whether rows have a delete button.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> to show the button when the resource can be deleted; otherwise,
    ///     <see langword="false" />. The default is <see langword="true" />.
    /// </value>
    public bool Delete { get; internal set; } = true;

    /// <summary>
    ///     Gets a value that indicates whether rows have an edit link or edit button.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> to show the link or button when the resource can be edited;
    ///     otherwise, <see langword="false" />. The default is <see langword="true" />.
    /// </value>
    public bool Edit { get; internal set; } = true;

    /// <summary>
    ///     Gets a value that indicates whether rows have a view link.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> to show the link when the resource has a view page; otherwise,
    ///     <see langword="false" />. The default is <see langword="true" />.
    /// </value>
    public bool View { get; internal set; } = true;

    internal ResourceRowActionsOptions() { }
}
