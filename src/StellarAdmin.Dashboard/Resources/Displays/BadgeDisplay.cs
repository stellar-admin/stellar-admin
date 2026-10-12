using StellarAdmin.TagHelpers;

namespace StellarAdmin.Dashboard.Resources.Displays;

/// <summary>
///     Displays a value as a badge, and each item of a collection as its own badge.
/// </summary>
/// <remarks>
///     To choose the badge by value, write a display template that switches on it.
/// </remarks>
public sealed class BadgeDisplay : FieldDisplay
{
    /// <inheritdoc />
    public override string TemplateName => "Displays/Badge";

    /// <summary>
    ///     The badge variant.
    /// </summary>
    /// <remarks>
    ///     Defaults to <see cref="BadgeVariant.Secondary" />.
    /// </remarks>
    public BadgeVariant Variant { get; set; } = BadgeVariant.Secondary;
}
