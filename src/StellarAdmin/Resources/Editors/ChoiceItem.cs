namespace StellarAdmin.Resources.Editors;

/// <summary>
///     A choice offered by a choice editor, or an item of a lookup editor. Editors display the fields they support
///     and ignore the rest.
/// </summary>
/// <param name="Value">The value posted when the choice is selected.</param>
/// <param name="Title">The choice's title.</param>
public sealed record ChoiceItem(string Value, string Title)
{
    /// <summary>
    ///     The secondary text displayed below the choice's title, or null.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    ///     Whether the choice can't be selected.
    /// </summary>
    public bool Disabled { get; init; }

    /// <summary>
    ///     The group the choice belongs to, or null. Choices with equal groups share one group.
    /// </summary>
    public ChoiceGroup? Group { get; init; }

    /// <summary>
    ///     The media displayed beside the choice's title, or null. Editors that can't display media ignore it.
    /// </summary>
    public ItemMedia? Media { get; init; }
}
