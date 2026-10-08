namespace StellarAdmin.Dashboard.Resources.Editors;

/// <summary>
///     A choice offered by a choice editor, or an item of a lookup editor. Editors display the fields they support
///     and ignore the rest.
/// </summary>
/// <param name="Value">The value posted when the choice is selected.</param>
/// <param name="Text">The choice's text.</param>
public sealed record ChoiceItem(string Value, string Text)
{
    /// <summary>
    ///     The secondary text displayed below the choice's text, or null.
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
    ///     The media displayed beside the choice's text, or null. Editors that can't display media ignore it.
    /// </summary>
    public ItemMedia? Media { get; init; }
}
