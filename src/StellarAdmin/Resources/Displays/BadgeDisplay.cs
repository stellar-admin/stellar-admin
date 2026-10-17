using StellarAdmin.UI;

namespace StellarAdmin.Resources.Displays;

/// <summary>
///     Shows a value as a badge.
/// </summary>
/// <remarks>
///     A collection shows a badge for each item. To choose the badge by value, write a display
///     template that switches on it.
/// </remarks>
public sealed class BadgeDisplay : FieldDisplay
{
    /// <inheritdoc />
    public override string TemplateName => "Displays/Badge";

    /// <summary>
    ///     Gets or sets the variant of the badge.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the variant. The default is
    ///     <see cref="BadgeVariant.Secondary" />.
    /// </value>
    public BadgeVariant Variant { get; set; } = BadgeVariant.Secondary;
}
