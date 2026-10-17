using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders the container that lays out a sidebar beside its main content.
/// </summary>
/// <remarks>
///     Place the sidebar, its triggers and the <c>&lt;sa-sidebar-inset&gt;</c> inside the wrapper.
///     Requires <c>stellar-admin.js</c>.
/// </remarks>
[HtmlTargetElement("sa-sidebar-wrapper")]
public sealed class SidebarWrapperTagHelper : StellarAdminTagHelperBase
{
    private const string SidebarWidth = "16rem";
    private const string SidebarWidthIcon = "3rem";
    private const string SidebarWidthMobile = "18rem";

    internal string? SidebarId { get; private set; }

    /// <inheritdoc />
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
            SidebarId = $"--sa-sidebar-{GetUniqueId(context)}";
            output.Attributes.SetAttribute("id", SidebarId);
        }

        output.Attributes.SetAttribute("data-slot", "sidebar-wrapper");
        output.Attributes.SetAttribute(
            "style",
            $"--sidebar-width: {SidebarWidth}; --sidebar-width-icon: {SidebarWidthIcon}; --sidebar-width-mobile: {SidebarWidthMobile}"
        );
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-sidebar-wrapper", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
