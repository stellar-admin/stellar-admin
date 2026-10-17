using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders a searchable menu of commands or options that the keyboard navigates from one
///     search input.
/// </summary>
/// <remarks>
///     Requires <c>stellar-admin.js</c>.
/// </remarks>
[HtmlTargetElement("sa-command")]
public sealed class CommandTagHelper : StellarAdminTagHelperBase
{
    /// <summary>
    ///     Gets or sets how the items are filtered as the user types.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies how the items are filtered. The default is
    ///     <see cref="CommandFilter.Client" />.
    /// </value>
    /// <remarks>
    ///     Use <see cref="CommandFilter.None" /> when the application supplies the matching items
    ///     itself.
    /// </remarks>
    [HtmlAttributeName("filter")]
    public CommandFilter? Filter { get; set; }

    /// <summary>
    ///     Gets or sets the visually hidden label of the search input.
    /// </summary>
    [HtmlAttributeName("label")]
    public string? Label { get; set; }

    /// <summary>
    ///     Gets or sets a value that indicates whether keyboard navigation wraps around the list.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> if moving past the last item goes to the first, and back;
    ///     otherwise, <see langword="false" />. The default is <see langword="false" />.
    /// </value>
    [HtmlAttributeName("loop")]
    public bool? Loop { get; set; }

    /// <inheritdoc />
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var effectiveFilter = Filter ?? CommandFilter.Client;
        var commandId =
            output.Attributes["id"]?.Value?.ToString() ?? $"sa-command-{GetUniqueId(context)}";
        var labelId = string.IsNullOrEmpty(Label) ? null : $"{commandId}-label";

        SetContext(
            context,
            new CommandContext
            {
                InputId = $"{commandId}-input",
                LabelId = labelId,
                ListId = $"{commandId}-list",
            }
        );

        output.TagName = "sel-command";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("id", commandId);
        output.Attributes.SetAttribute("data-slot", "command");
        output.Attributes.SetAttribute("data-filter", effectiveFilter.GetDataAttributeText());
        if (Loop == true)
        {
            output.Attributes.SetAttribute("data-loop", "true");
        }

        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-command", output.GetUserSuppliedClass())
        );

        if (labelId is not null)
        {
            var label = new TagBuilder("label");
            label.Attributes["id"] = labelId;
            label.Attributes["class"] = "sr-only";
            label.InnerHtml.Append(Label!);
            output.PreContent.AppendHtml(label);
        }

        return Task.CompletedTask;
    }
}
