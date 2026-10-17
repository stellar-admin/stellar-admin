namespace StellarAdmin.Resources.Editors;

/// <summary>
///     Represents a group of choices.
/// </summary>
/// <param name="Text">The group's heading.</param>
/// <remarks>
///     Editors that display groups show each group at the position of its first choice.
/// </remarks>
public sealed record ChoiceGroup(string Text)
{
    /// <summary>
    ///     Gets or sets the secondary text displayed below the group's heading.
    /// </summary>
    /// <value>
    ///     The secondary text, or <see langword="null" /> for none. The default is
    ///     <see langword="null" />.
    /// </value>
    public string? Description { get; init; }

    /// <summary>
    ///     Gets or sets a value that indicates whether the group is disabled.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> if none of the group's choices can be selected; otherwise,
    ///     <see langword="false" />. The default is <see langword="false" />.
    /// </value>
    public bool Disabled { get; init; }
}
