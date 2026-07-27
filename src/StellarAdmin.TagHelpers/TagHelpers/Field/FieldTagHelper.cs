using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     Wraps a form control together with its label, description, and error message, arranging them
///     according to the chosen orientation.
/// </summary>
[HtmlTargetElement("sa-field")]
public class FieldTagHelper : StellarAdminTagHelperBase
{
    /// <summary>
    ///     How the field arranges its label, control, and supporting text.
    /// </summary>
    /// <remarks>
    ///     Defaults to <see cref="FieldOrientation.Vertical" />.
    /// </remarks>
    [HtmlAttributeName("orientation")]
    public FieldOrientation? Orientation { get; set; }

    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        // The MergeAttributes call below does a simple concatenation of the classes, but this is not correct.
        // We need to compose the class string, whil happen inside the FieldTagBuilder.
        // To ensure correct behaviour we need to first extract the user supplied class so we can pass it to
        // FieldTagBuilder and then clear it before calling MergeAttribute() to prevent a double merge.
        var userSuppliedClass = output.GetUserSuppliedClass();
        output.Attributes.SetAttribute("class", string.Empty);

        var effectiveOrientation = Orientation ?? FieldOrientation.Vertical;

        var tagBuilder = new FieldTagBuilder(effectiveOrientation, userSuppliedClass);

        output.TagName = tagBuilder.TagName;
        output.TagMode = TagMode.StartTagAndEndTag;

        output.MergeAttributes(tagBuilder);

        return Task.CompletedTask;
    }
}
