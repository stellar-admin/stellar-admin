using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     A small label used to highlight status, counts, or categories.
/// </summary>
[HtmlTargetElement("sa-badge")]
public class BadgeTagHelper : StellarAdminTagHelperBase
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
    ///     The visual style of the badge.
    /// </summary>
    /// <remarks>
    ///     Defaults to <see cref="BadgeVariant.Default" />.
    /// </remarks>
    [HtmlAttributeName("variant")]
    public BadgeVariant? Variant { get; set; }

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
                "group/badge",
                BadgeVariantClasses[effectiveVariant],
                output.GetUserSuppliedClass()
            )
        );

        return Task.CompletedTask;
    }
}
