using Microsoft.AspNetCore.Razor.TagHelpers;
using StellarAdmin.UI.Theming;

namespace StellarAdmin.UI.TagHelpers;

/// <summary>
///     A small label used to highlight status, counts, or categories.
/// </summary>
[HtmlTargetElement("sa-badge")]
public class BadgeTagHelper : StellarAdminTagHelperBase
{
    public BadgeTagHelper(ICssClassMerger classMerger)
        : base(classMerger) { }

    private static readonly Dictionary<BadgeVariant, ThemeToken> BadgeVariantClasses = new()
    {
        [BadgeVariant.Default] = new ThemeToken("sa-badge-variant-default"),
        [BadgeVariant.Secondary] = new ThemeToken("sa-badge-variant-secondary"),
        [BadgeVariant.Destructive] = new ThemeToken("sa-badge-variant-destructive"),
        [BadgeVariant.Outline] = new ThemeToken("sa-badge-variant-outline"),
        [BadgeVariant.Ghost] = new ThemeToken("sa-badge-variant-ghost"),
        [BadgeVariant.Link] = new ThemeToken("sa-badge-variant-link"),
    };

    /// <summary>
    ///     The visual style of the badge.
    /// </summary>
    /// <remarks>
    ///     Defaults to <see cref="BadgeVariant.Default" />.
    /// </remarks>
    [HtmlAttributeName("variant")]
    public BadgeVariant? Variant { get; set; }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var effectiveVariant = Variant ?? BadgeVariant.Default;

        output.TagName = "span";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "badge");
        output.Attributes.SetAttribute(
            "class",
            BuildClassString(
                new ThemeToken("sa-badge"),
                "group/badge",
                BadgeVariantClasses[effectiveVariant],
                output.GetUserSuppliedClass()
            )
        );

        output.Content.SetHtmlContent(await output.GetChildContentAsync());
    }
}
