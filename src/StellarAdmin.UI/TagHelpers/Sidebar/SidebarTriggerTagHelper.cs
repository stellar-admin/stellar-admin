using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Options;
using StellarAdmin.UI.Icons;

namespace StellarAdmin.UI;

/// <summary>
///     Renders a button that expands and collapses the sidebar.
/// </summary>
/// <remarks>
///     Place the button in the same <c>&lt;sa-sidebar-wrapper&gt;</c> as the sidebar. The button
///     shows a toggle icon unless you give it content. Requires <c>stellar-admin.js</c>.
/// </remarks>
[HtmlTargetElement("sa-sidebar-trigger")]
public class SidebarTriggerTagHelper : StellarAdminTagHelperBase
{
    private readonly IconOptions _iconOptions;

    /// <summary>
    ///     Initializes a new instance of the <see cref="SidebarTriggerTagHelper" /> class.
    /// </summary>
    /// <param name="iconOptions">The icon options that supply the toggle icon.</param>
    public SidebarTriggerTagHelper(IOptions<IconOptions> iconOptions)
    {
        _iconOptions = iconOptions.Value;
    }

    /// <inheritdoc />
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "button";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("type", "button");
        output.Attributes.SetAttribute("data-slot", "sidebar-trigger");
        output.Attributes.SetAttribute("aria-label", "Toggle Sidebar");

        // Target the parent `sel-sidebar` via the native command API. When clicked,
        // the button dispatches a `command` event on that element, which toggles it.
        var sidebarId = GetParentTagHelper<SidebarWrapperTagHelper>()?.SidebarId;
        if (sidebarId != null)
        {
            output.Attributes.SetAttribute("command", "--toggle-sidebar");
            output.Attributes.SetAttribute("commandfor", sidebarId);
        }

        // Default icon, unless the user supplied their own content.
        TagHelperContent iconContent;
        var childContent = await output.GetChildContentAsync();
        if (!childContent.IsEmptyOrWhiteSpace)
        {
            iconContent = childContent;
        }
        else
        {
            var iconOutput = new TagHelperOutput(
                "svg",
                [new TagHelperAttribute("class", "size-4")],
                (_, _) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent())
            );
            var iconTagHelper = new IconTagHelper(_iconOptions)
            {
                Name = _iconOptions.GetSemanticIconName(SemanticIconRole.ToggleSidebar),
            };
            iconTagHelper.Process(context, iconOutput);
            iconContent = new DefaultTagHelperContent().AppendHtml(iconOutput);
        }

        ButtonRenderingHelper.RenderAttributes(output, ButtonVariant.Ghost, ButtonSize.IconSmall);

        output.Content.SetHtmlContent(iconContent);
    }
}
