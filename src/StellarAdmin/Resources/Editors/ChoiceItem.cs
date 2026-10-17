namespace StellarAdmin.Resources.Editors;

/// <summary>
///     Represents a choice of a choice editor or an item of a lookup editor.
/// </summary>
/// <param name="Value">The value posted when the choice is selected.</param>
/// <param name="Title">The choice's title.</param>
/// <remarks>
///     Editors display the properties they support and ignore the rest.
/// </remarks>
public sealed record ChoiceItem(string Value, string Title)
{
    /// <summary>
    ///     Gets or sets the secondary text displayed below the choice's title.
    /// </summary>
    /// <value>
    ///     The secondary text, or <see langword="null" /> for none. The default is
    ///     <see langword="null" />.
    /// </value>
    public string? Description { get; init; }

    /// <summary>
    ///     Gets or sets a value that indicates whether the choice is disabled.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> if the choice can't be selected; otherwise,
    ///     <see langword="false" />. The default is <see langword="false" />.
    /// </value>
    public bool Disabled { get; init; }

    /// <summary>
    ///     Gets or sets the group the choice belongs to.
    /// </summary>
    /// <value>
    ///     The group, or <see langword="null" /> for none. The default is <see langword="null" />.
    /// </value>
    /// <remarks>
    ///     Choices with equal groups share one group.
    /// </remarks>
    public ChoiceGroup? Group { get; init; }

    /// <summary>
    ///     Gets or sets the media displayed beside the choice's title.
    /// </summary>
    /// <value>
    ///     The media, or <see langword="null" /> for none. The default is <see langword="null" />.
    /// </value>
    /// <remarks>
    ///     Editors that can't display media ignore it.
    /// </remarks>
    public ItemMedia? Media { get; init; }
}
