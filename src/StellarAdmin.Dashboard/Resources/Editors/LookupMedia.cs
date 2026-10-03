namespace StellarAdmin.Dashboard.Resources.Editors;

/// <summary>
///     The media displayed beside a lookup item's title.
/// </summary>
/// <param name="Type">The kind of media.</param>
/// <param name="Value">The code text, or the avatar's image URL; null displays an avatar's initials.</param>
public sealed record LookupMedia(LookupMediaType Type, string? Value);
