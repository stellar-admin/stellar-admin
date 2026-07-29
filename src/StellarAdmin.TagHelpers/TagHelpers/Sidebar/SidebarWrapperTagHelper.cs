using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     The outermost sidebar container that provides layout and shared state for the sidebar and its inset content.
///     Renders the <c>sel-sidebar</c> web component that nested triggers and the backdrop toggle.
/// </summary>
[HtmlTargetElement("sa-sidebar-wrapper")]
public class SidebarWrapperTagHelper : StellarAdminTagHelperBase
{
    private const string SidebarWidth = "16rem";
    private const string SidebarWidthIcon = "3rem";
    private const string SidebarWidthMobile = "18rem";

    /// <summary>
    ///     The id of the rendered <c>sel-sidebar</c> element. Nested triggers and the
    ///     backdrop read this (via <see cref="StellarAdminTagHelperBase.GetParentTagHelper{T}" />)
    ///     to target it with <c>commandfor</c>.
    /// </summary>
    [HtmlAttributeNotBound]
    public string? SidebarId { get; private set; }

    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        // The wrapper renders the `sel-sidebar` web component, which acts as the
        // state provider. Nested triggers toggle it via the native command API.
        output.TagName = "sel-sidebar";
        output.TagMode = TagMode.StartTagAndEndTag;

        // Resolve the id before child content is processed so nested triggers can
        // read it. Honour a user-supplied id; otherwise generate a stable one.
        SidebarId = output.Attributes.TryGetAttribute("id", out var idAttribute)
            ? idAttribute.Value.ToString()
            : null;
        if (SidebarId == null)
        {
            SidebarId = $"--sa-sidebar-{context.UniqueId}";
            output.Attributes.SetAttribute("id", SidebarId);
        }

        output.Attributes.SetAttribute("data-slot", "sidebar-wrapper");
        output.Attributes.SetAttribute(
            "style",
            $"--sidebar-width: {SidebarWidth}; --sidebar-width-icon: {SidebarWidthIcon}; --sidebar-width-mobile: {SidebarWidthMobile}"
        );
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses(
                "sa-sidebar-wrapper",
                "group/sidebar-wrapper",
                output.GetUserSuppliedClass()
            )
        );

        return Task.CompletedTask;
    }
}
