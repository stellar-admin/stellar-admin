using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders a small label that highlights a status, count or category.
/// </summary>
[HtmlTargetElement("sa-badge")]
public sealed class BadgeTagHelper : StellarAdminTagHelperBase
{
    private static readonly Dictionary<BadgeVariant, string> BadgeVariantClasses = new()
    {
        [BadgeVariant.Default] = "sa-badge-variant-default",
        [BadgeVariant.Secondary] = "sa-badge-variant-secondary",
        [BadgeVariant.Destructive] = "sa-badge-variant-destructive",
        [BadgeVariant.Outline] = "sa-badge-variant-outline",
        [BadgeVariant.Ghost] = "sa-badge-variant-ghost",
        [BadgeVariant.Link] = "sa-badge-variant-link",
    };

    /// <summary>
    ///     Gets or sets the visual style of the badge.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the style. The default is
    ///     <see cref="BadgeVariant.Default" />.
    /// </value>
    [HtmlAttributeName("variant")]
    public BadgeVariant? Variant { get; set; }

    /// <inheritdoc />
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var effectiveVariant = Variant ?? BadgeVariant.Default;

        output.TagName = "span";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "badge");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses(
                "sa-badge",
                BadgeVariantClasses[effectiveVariant],
                output.GetUserSuppliedClass()
            )
        );

        return Task.CompletedTask;
    }
}
