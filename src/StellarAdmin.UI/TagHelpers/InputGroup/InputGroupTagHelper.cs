using Microsoft.AspNetCore.Razor.TagHelpers;
using StellarAdmin.UI.Theming;

namespace StellarAdmin.UI.TagHelpers;

/// <summary>
///     A container that groups an input with add-ons, buttons, or text so they render as a single
///     combined field.
/// </summary>
[HtmlTargetElement("sa-input-group")]
public class InputGroupTagHelper : StellarAdminTagHelperBase
{
    public InputGroupTagHelper(ICssClassMerger classMerger)
        : base(classMerger) { }

    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("role", "group");
        output.Attributes.SetAttribute("data-slot", "input-group");

        output.Attributes.SetAttribute(
            "class",
            ClassMerger.Merge(
                new ThemeToken("sa-input-group"),
                "group/input-group relative flex w-full min-w-0 items-center outline-none has-[>textarea]:h-auto",
                output.GetUserSuppliedClass()
            )
        );

        return Task.CompletedTask;
    }
}
