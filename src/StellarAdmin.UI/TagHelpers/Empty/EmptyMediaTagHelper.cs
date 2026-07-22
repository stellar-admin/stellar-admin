using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI.TagHelpers;

/// <summary>
///     The media region of an empty state, displaying an icon or illustration above the title.
/// </summary>
[HtmlTargetElement("sa-empty-media")]
public class EmptyMediaTagHelper : StellarAdminTagHelperBase
{
    private static readonly Dictionary<EmptyMediaVariant, string> VariantClasses = new Dictionary<
        EmptyMediaVariant,
        string
    >
    {
        [EmptyMediaVariant.Default] = "sa-empty-media-default",
        [EmptyMediaVariant.Icon] = "sa-empty-media-icon",
    };

    /// <summary>
    ///     The visual style of the media region.
    /// </summary>
    /// <remarks>
    ///     Defaults to <see cref="EmptyMediaVariant.Default" />.
    /// </remarks>
    [HtmlAttributeName("variant")]
    public EmptyMediaVariant? Variant { get; set; }

    public EmptyMediaTagHelper(ICssClassMerger classMerger)
        : base(classMerger) { }

    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var effectiveVariant = Variant ?? EmptyMediaVariant.Default;

        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "empty-icon");
        output.Attributes.SetAttribute("data-variant", effectiveVariant.GetDataAttributeText());
        output.Attributes.SetAttribute(
            "class",
            ClassMerger.Merge(
                "sa-empty-media",
                VariantClasses[effectiveVariant],
                output.GetUserSuppliedClass()
            )
        );

        return Task.CompletedTask;
    }
}
