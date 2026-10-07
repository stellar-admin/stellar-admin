namespace StellarAdmin.Dashboard.Resources.Editors;

/// <summary>
///     A group of choices. Editors that display groups gather each group's choices at the position of its first choice.
/// </summary>
/// <param name="Text">The group's heading.</param>
public sealed record ChoiceGroup(string Text)
{
    /// <summary>
    ///     The secondary text displayed below the group's heading, or null.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    ///     Whether none of the group's choices can be selected.
    /// </summary>
    public bool Disabled { get; init; }
}
