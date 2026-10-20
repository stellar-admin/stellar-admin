using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders a list of steps or tasks, each with a status.
/// </summary>
[HtmlTargetElement("sa-checklist")]
public sealed class ChecklistTagHelper : StellarAdminTagHelperBase
{
    /// <summary>
    ///     Gets or sets the line drawn between the rows.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the line. The default is
    ///     <see cref="ChecklistDivider.Inset" />.
    /// </value>
    [HtmlAttributeName("divider")]
    public ChecklistDivider? Divider { get; set; }

    /// <summary>
    ///     Gets or sets the size of the checklist, which sets the size of its status icons and the
    ///     spacing between its rows.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the size. The default is
    ///     <see cref="ChecklistSize.Default" />.
    /// </value>
    [HtmlAttributeName("size")]
    public ChecklistSize? Size { get; set; }

    /// <inheritdoc />
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var effectiveDivider = Divider ?? ChecklistDivider.Inset;
        var effectiveSize = Size ?? ChecklistSize.Default;

        output.TagName = "ul";
        output.TagMode = TagMode.StartTagAndEndTag;

        // A list styled without bullets loses its list semantics in Safari unless it has the role.
        output.Attributes.SetAttribute("role", "list");
        output.Attributes.SetAttribute("data-slot", "checklist");
        output.Attributes.SetAttribute("data-size", effectiveSize.GetDataAttributeText());
        output.Attributes.SetAttribute("data-divider", effectiveDivider.GetDataAttributeText());
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-checklist", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
