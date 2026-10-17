using System.Globalization;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders a slot that shows one character of the code in an <c>&lt;sa-input-otp&gt;</c>.
/// </summary>
[HtmlTargetElement("sa-input-otp-slot")]
public class InputOtpSlotTagHelper : StellarAdminTagHelperBase
{
    /// <summary>
    ///     Gets or sets the position of the character that the slot shows.
    /// </summary>
    /// <value>
    ///     A zero-based position. The default is the next position in document order.
    /// </value>
    [HtmlAttributeName("index")]
    public int? Index { get; set; }

    /// <inheritdoc />
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var inputOtpContext = GetContext<InputOtpContext>(context);

        int index;
        if (Index.HasValue)
        {
            index = Index.Value;
        }
        else if (inputOtpContext != null)
        {
            index = inputOtpContext.NextIndex;
            inputOtpContext.NextIndex++;
        }
        else
        {
            index = 0;
        }

        var hasError = inputOtpContext?.HasError ?? false;

        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "input-otp-slot");
        output.Attributes.SetAttribute("data-index", index.ToString(CultureInfo.InvariantCulture));
        output.Attributes.SetAttribute("data-active", "false");
        if (hasError)
        {
            output.Attributes.SetAttribute("aria-invalid", "true");
        }
        output.Attributes.SetAttribute(
            "class",
            InputOtpRenderer.SlotClass(
                JoinCssClasses(inputOtpContext?.ClassNames?.Slot, output.GetUserSuppliedClass())
            )
        );

        // Render the character and the specified index
        var character = inputOtpContext?.CharAt(index);
        if (!string.IsNullOrEmpty(character))
        {
            output.Content.SetContent(character);
        }
        else
        {
            output.Content.SetContent(string.Empty);
        }
    }
}
