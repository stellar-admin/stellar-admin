using Microsoft.AspNetCore.Razor.TagHelpers;
using StellarAdmin.UI.Icons;

namespace StellarAdmin.UI.TagHelpers;

/// <summary>
///     A button that toggles the open or collapsed state of its parent sidebar.
/// </summary>
[HtmlTargetElement("sa-sidebar-trigger")]
public class SidebarTriggerTagHelper : StellarAdminTagHelperBase
{
    private readonly IIconManager _iconManager;

    public SidebarTriggerTagHelper(IIconManager iconManager)
    {
        _iconManager = iconManager ?? throw new ArgumentNullException(nameof(iconManager));
    }

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
            var iconTagHelper = new IconTagHelper(_iconManager) { Name = "panel-left" };
            iconTagHelper.Process(context, iconOutput);
            iconContent = new DefaultTagHelperContent().AppendHtml(iconOutput);
        }

        ButtonRenderingHelper.RenderAttributes(output, ButtonVariant.Ghost, ButtonSize.IconSmall);

        output.Content.SetHtmlContent(iconContent);
    }
}
