using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Options;
using StellarAdmin.Icons;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     The search input of a command menu. Attributes the tag does not recognize, such as
///     <c>name</c>, <c>placeholder</c>, or request attributes for server-side search, are rendered
///     on the input element.
/// </summary>
[HtmlTargetElement("sa-command-input", TagStructure = TagStructure.WithoutEndTag)]
public class CommandInputTagHelper : StellarAdminTagHelperBase
{
    private readonly IconOptions _iconOptions;

    public CommandInputTagHelper(IOptions<IconOptions> iconOptions)
    {
        _iconOptions = iconOptions.Value;
    }

    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var commandContext = GetContext<CommandContext>(context);

        output.TagName = "input";
        output.TagMode = TagMode.StartTagOnly;

        if (!output.Attributes.ContainsName("type"))
        {
            output.Attributes.SetAttribute("type", "text");
        }

        if (commandContext is not null)
        {
            if (!output.Attributes.ContainsName("id"))
            {
                output.Attributes.SetAttribute("id", commandContext.InputId);
            }

            output.Attributes.SetAttribute("aria-controls", commandContext.ListId);
            if (
                commandContext.LabelId is not null
                && !output.Attributes.ContainsName("aria-labelledby")
            )
            {
                output.Attributes.SetAttribute("aria-labelledby", commandContext.LabelId);
            }
        }

        output.Attributes.SetAttribute("role", "combobox");
        output.Attributes.SetAttribute("aria-autocomplete", "list");
        output.Attributes.SetAttribute("aria-expanded", "true");
        output.Attributes.SetAttribute("autocomplete", "off");
        output.Attributes.SetAttribute("autocorrect", "off");
        output.Attributes.SetAttribute("spellcheck", "false");
        output.Attributes.SetAttribute("data-slot", "command-input");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-command-input", output.GetUserSuppliedClass())
        );

        var wrapper = new TagBuilder("div");
        wrapper.Attributes["data-slot"] = "command-input-wrapper";
        wrapper.Attributes["class"] = "sa-command-input-wrapper";

        var inputGroup = new TagBuilder("div");
        inputGroup.Attributes["role"] = "group";
        inputGroup.Attributes["data-slot"] = "input-group";
        inputGroup.Attributes["class"] = JoinCssClasses("sa-input-group", "sa-command-input-group");

        var addon = new TagBuilder("div");
        addon.Attributes["role"] = "group";
        addon.Attributes["data-slot"] = "input-group-addon";
        addon.Attributes["data-align"] = "inline-start";
        addon.Attributes["class"] = JoinCssClasses(
            "sa-input-group-addon",
            "sa-input-group-addon-align-inline-start"
        );
        addon.InnerHtml.AppendHtml(
            CommandRenderingHelper.RenderIcon(
                context,
                _iconOptions,
                SemanticIconRole.Search,
                "sa-command-input-icon"
            )
        );

        output.PreElement.AppendHtml(wrapper.RenderStartTag());
        output.PreElement.AppendHtml(inputGroup.RenderStartTag());
        output.PostElement.AppendHtml(addon);
        output.PostElement.AppendHtml(inputGroup.RenderEndTag());
        output.PostElement.AppendHtml(wrapper.RenderEndTag());

        return Task.CompletedTask;
    }
}
