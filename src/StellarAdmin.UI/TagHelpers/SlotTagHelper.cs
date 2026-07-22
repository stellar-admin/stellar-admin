using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI.TagHelpers;

/// <summary>
///     Assigns its child content to a named slot on the parent StellarAdmin.UI tag helper, letting a
///     parent render caller-supplied content in a specific location.
/// </summary>
[HtmlTargetElement("sa-slot", TagStructure = TagStructure.NormalOrSelfClosing)]
public class SlotTagHelper : StellarAdminTagHelperBase
{
    /// <summary>
    ///     The name of the slot to populate on the parent tag helper.
    /// </summary>
    [HtmlAttributeName("name")]
    public required string Name { get; set; }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            throw new ArgumentException("The 'name' attribute is required on a slot.");
        }

        if (ParentTagHelper is null)
        {
            throw new InvalidOperationException(
                "A slot Tag Helper can only be used inside a StellarAdmin.UI Tag Helper."
            );
        }

        var childContent = await output.GetChildContentAsync();

        if (!ParentTagHelper.TryAddNamedSlot(Name, childContent))
        {
            throw new ArgumentException(
                $"The slot named '{Name}' has already been added. You cannot add the same slot multiple times."
            );
        }

        output.SuppressOutput();
    }
}
