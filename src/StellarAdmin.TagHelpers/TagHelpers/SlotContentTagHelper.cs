using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     Assigns its child content to a named slot on the nearest StellarAdmin tag helper,
///     letting that tag helper render caller-supplied content in a specific location.
/// </summary>
[HtmlTargetElement("sa-slot-content", TagStructure = TagStructure.NormalOrSelfClosing)]
public class SlotContentTagHelper : StellarAdminTagHelperBase
{
    /// <summary>
    ///     The name of the slot to populate on the nearest StellarAdmin tag helper.
    /// </summary>
    [HtmlAttributeName("name")]
    public required string Name { get; set; }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            throw new ArgumentException("The 'name' attribute is required on <sa-slot-content>.");
        }

        if (ParentTagHelper is null)
        {
            throw new InvalidOperationException(
                "<sa-slot-content> can only be used inside a StellarAdmin Tag Helper."
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
