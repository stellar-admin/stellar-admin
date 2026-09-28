using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     A searchable menu of commands or options, navigated with the keyboard from a single search
///     input.
/// </summary>
[HtmlTargetElement("sa-command")]
public class CommandTagHelper : StellarAdminTagHelperBase
{
    /// <summary>
    ///     How items are narrowed as the user types.
    /// </summary>
    /// <remarks>
    ///     Defaults to <see cref="CommandFilter.Client" />. Use <see cref="CommandFilter.None" />
    ///     when the application supplies matching items itself.
    /// </remarks>
    [HtmlAttributeName("filter")]
    public CommandFilter? Filter { get; set; }

    /// <summary>
    ///     A visually hidden label that names the search input for assistive technology.
    /// </summary>
    [HtmlAttributeName("label")]
    public string? Label { get; set; }

    /// <summary>
    ///     Whether keyboard navigation wraps from the last item to the first and back.
    /// </summary>
    /// <remarks>
    ///     Defaults to <c>false</c>.
    /// </remarks>
    [HtmlAttributeName("loop")]
    public bool? Loop { get; set; }

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
