using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders a sidebar that holds a header, content and footer.
/// </summary>
/// <remarks>
///     On wide screens the sidebar is a fixed panel that can collapse. On narrow screens it opens
///     as a drawer over the page. Place it in a <c>&lt;sa-sidebar-wrapper&gt;</c>. Requires
///     <c>stellar-admin.js</c>.
/// </remarks>
[HtmlTargetElement("sa-sidebar")]
public sealed class SidebarTagHelper : StellarAdminTagHelperBase
{
    /// <summary>
    ///     Gets or sets the visual style of the sidebar.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the style. The default is
    ///     <see cref="SidebarVariant.Default" />.
    /// </value>
    [HtmlAttributeName("variant")]
    public SidebarVariant? Variant { get; set; }

    /// <summary>
    ///     Gets or sets the edge of the screen that the sidebar sits on.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the edge. The default is
    ///     <see cref="SidebarSide.Left" />.
    /// </value>
    [HtmlAttributeName("side")]
    public SidebarSide? Side { get; set; }

    /// <summary>
    ///     Gets or sets how the sidebar collapses.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies how the sidebar collapses. The default is
    ///     <see cref="SidebarCollapsible.OffCanvas" />.
    /// </value>
    /// <remarks>
    ///     A sidebar that can't collapse is always shown and doesn't open as a drawer on narrow
    ///     screens.
    /// </remarks>
    [HtmlAttributeName("collapsible")]
    public SidebarCollapsible? Collapsible { get; set; }

    /// <inheritdoc />
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var effectiveVariant = Variant ?? SidebarVariant.Default;
        var effectiveSide = Side ?? SidebarSide.Left;
        var effectiveCollapsible = Collapsible ?? SidebarCollapsible.OffCanvas;

        var isFloatingOrInset = effectiveVariant is SidebarVariant.Floating or SidebarVariant.Inset;

        // Capture before the wrapper's own class is set below, otherwise the container
        // would inherit "sa-sidebar" instead of the author's class.
        var userSuppliedClass = output.GetUserSuppliedClass();

        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "sidebar");
        output.Attributes.SetAttribute("data-variant", effectiveVariant.GetDataAttributeText());
        output.Attributes.SetAttribute("data-side", effectiveSide.GetDataAttributeText());

        // collapsible="none": a static, always-visible sidebar with no toggle and
        // no mobile drawer.
        if (effectiveCollapsible == SidebarCollapsible.None)
        {
            output.Attributes.SetAttribute(
                "data-collapsible",
                effectiveCollapsible.GetDataAttributeText()
            );
            output.Attributes.SetAttribute(
                "class",
                JoinCssClasses("sa-sidebar-inner", userSuppliedClass)
            );

            output.Content.AppendHtml(await output.GetChildContentAsync());
            return;
        }

        // Initial state — the `sel-sidebar` provider keeps these in sync at runtime.
        output.Attributes.SetAttribute("data-state", "expanded");
        output.Attributes.SetAttribute("data-mobile", "closed");
        // data-collapsible is dynamic (set to the mode only while collapsed on desktop);
        // the provider reads the static mode from data-collapsible-config.
        output.Attributes.SetAttribute("data-collapsible", "");
        output.Attributes.SetAttribute(
            "data-collapsible-config",
            effectiveCollapsible.GetDataAttributeText()
        );
        output.Attributes.SetAttribute("class", "sa-sidebar");

        /* Backdrop — mobile only. Fades in behind the drawer and closes it on click.
           A <button> so the native command API fires; targets the parent sel-sidebar. */
        var sidebarId = GetParentTagHelper<SidebarWrapperTagHelper>()?.SidebarId;
        var backdropTagBuilder = new TagBuilder("button");
        backdropTagBuilder.Attributes.Add("type", "button");
        backdropTagBuilder.Attributes.Add("data-slot", "sidebar-backdrop");
        backdropTagBuilder.Attributes.Add("aria-label", "Close sidebar");
        if (sidebarId != null)
        {
            backdropTagBuilder.Attributes.Add("command", "--close-mobile");
            backdropTagBuilder.Attributes.Add("commandfor", sidebarId);
        }
        backdropTagBuilder.Attributes.Add("class", "sa-sidebar-backdrop");
        output.Content.AppendHtml(backdropTagBuilder);

        /* Gap — desktop-only spacer that pushes the inset content. */
        var gapTagBuilder = new TagBuilder("div");
        gapTagBuilder.Attributes.Add("data-slot", "sidebar-gap");
        gapTagBuilder.Attributes.Add("class", JoinCssClasses("sa-sidebar-gap"));
        output.Content.AppendHtml(gapTagBuilder);

        /* Sidebar container — desktop panel + mobile drawer in one element.
           Desktop  : anchored by left/right; collapses via left/right offset (offcanvas)
                      or width (icon). data-collapsible is only set on desktop.
           Mobile   : off-canvas drawer driven by translateX (max-md only), so the
                      desktop left/right rules never apply at mobile widths. */
        var sidebarContainerTagBuilder = new TagBuilder("div");
        sidebarContainerTagBuilder.Attributes.Add("data-slot", "sidebar-container");
        sidebarContainerTagBuilder.Attributes.Add(
            "class",
            JoinCssClasses("sa-sidebar-container", userSuppliedClass)
        );

        /* Sidebar inner */
        var sidebarInnerTagBuilder = new TagBuilder("div");
        sidebarInnerTagBuilder.Attributes.Add("data-sidebar", "sidebar");
        sidebarInnerTagBuilder.Attributes.Add("data-slot", "sidebar-inner");
        sidebarInnerTagBuilder.Attributes.Add("class", JoinCssClasses("sa-sidebar-inner"));
        sidebarInnerTagBuilder.InnerHtml.AppendHtml(await output.GetChildContentAsync());

        sidebarContainerTagBuilder.InnerHtml.AppendHtml(sidebarInnerTagBuilder);

        output.Content.AppendHtml(sidebarContainerTagBuilder);
    }
}
