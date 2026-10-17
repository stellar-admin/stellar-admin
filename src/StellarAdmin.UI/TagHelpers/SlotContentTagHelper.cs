using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Supplies content for a named slot of the StellarAdmin tag helper that contains it.
/// </summary>
/// <remarks>
///     The element renders nothing in place. The containing tag helper renders the content where
///     its template places the slot.
/// </remarks>
[HtmlTargetElement("sa-slot-content", TagStructure = TagStructure.NormalOrSelfClosing)]
public class SlotContentTagHelper : StellarAdminTagHelperBase
{
    /// <summary>
    ///     Gets or sets the name of the slot that the content fills.
    /// </summary>
    [HtmlAttributeName("name")]
    public required string Name { get; set; }

    /// <inheritdoc />
    /// <exception cref="ArgumentException">
    ///     <see cref="Name" /> is empty or white space, or the slot already has content.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///     The element isn't inside a StellarAdmin tag helper.
    /// </exception>
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
