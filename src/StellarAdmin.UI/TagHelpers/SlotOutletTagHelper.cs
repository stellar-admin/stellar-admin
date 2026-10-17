using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders the content supplied for a named slot.
/// </summary>
/// <remarks>
///     Use inside the view of a templated tag helper. The element renders its own child content
///     when no <c>&lt;sa-slot-content&gt;</c> fills the slot.
/// </remarks>
[HtmlTargetElement("sa-slot-outlet", TagStructure = TagStructure.NormalOrSelfClosing)]
public class SlotOutletTagHelper : StellarAdminTagHelperBase
{
    /// <summary>
    ///     Gets or sets the name of the slot to render.
    /// </summary>
    [HtmlAttributeName("name")]
    public required string Name { get; set; }

    /// <summary>
    ///     Gets or sets the context of the executing view.
    /// </summary>
    [HtmlAttributeNotBound]
    [ViewContext]
    public required ViewContext ViewContext { get; set; }

    /// <inheritdoc />
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
