using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     Renders the content an <c>&lt;sa-slot-content&gt;</c> element assigned to a named
///     slot, or its own child content as a fallback when the slot is unfilled. Used inside
///     the view of a templated tag helper.
/// </summary>
[HtmlTargetElement("sa-slot-outlet", TagStructure = TagStructure.NormalOrSelfClosing)]
public class SlotOutletTagHelper : StellarAdminTagHelperBase
{
    /// <summary>
    ///     The name of the slot whose content to render.
    /// </summary>
    [HtmlAttributeName("name")]
    public required string Name { get; set; }

    [HtmlAttributeNotBound]
    [ViewContext]
    public required ViewContext ViewContext { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = null;

        if (
            ViewContext.ViewData.TryGetValue(
                StellarAdminTemplatedTagHelperBase.SlotHostViewDataKey,
                out var value
            )
            && value is StellarAdminTagHelperBase host
            && host.TryGetNamedSlot(Name, out var content)
        )
        {
            // Setting the content also means the fallback children never execute.
            output.Content.SetHtmlContent(content);
        }
    }
}
