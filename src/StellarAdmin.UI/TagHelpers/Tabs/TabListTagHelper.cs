using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Options;
using StellarAdmin.UI.Icons;

namespace StellarAdmin.UI;

/// <summary>
///     Renders a list of tabs.
/// </summary>
/// <remarks>
///     The list holds <c>&lt;sa-tab-link&gt;</c> elements that link to other pages. Inside a
///     <c>&lt;sa-tabs&gt;</c>, it holds <c>&lt;sa-tab&gt;</c> elements that switch between
///     panels. A horizontal list moves the tabs that don't fit into a More menu at its end,
///     unless <see cref="Overflow" /> is <see cref="TabListOverflow.None" />.
/// </remarks>
[HtmlTargetElement("sa-tab-list")]
public sealed class TabListTagHelper : StellarAdminTagHelperBase
{
    private readonly IconOptions _iconOptions;

    /// <summary>
    ///     Initializes a new instance of the <see cref="TabListTagHelper" /> class.
    /// </summary>
    /// <param name="iconOptions">The icon options that supply the More menu's icon.</param>
    public TabListTagHelper(IOptions<IconOptions> iconOptions)
    {
        _iconOptions = iconOptions.Value;
    }

    /// <summary>
    ///     Gets or sets the text of the button that opens the menu of the tabs that don't fit.
    /// </summary>
    /// <value>
    ///     The button's text. The default is "More".
    /// </value>
    [HtmlAttributeName("more-label")]
    public string? MoreLabel { get; set; }

    /// <summary>
    ///     Gets or sets the direction in which the tabs are arranged.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the direction. The default is
    ///     <see cref="TabListOrientation.Horizontal" />.
    /// </value>
    [HtmlAttributeName("orientation")]
    public TabListOrientation? Orientation { get; set; }

    /// <summary>
    ///     Gets or sets what the list does with the tabs that don't fit on one line.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the behavior. The default is
    ///     <see cref="TabListOverflow.Menu" />. A vertical list ignores it.
    /// </value>
    [HtmlAttributeName("overflow")]
    public TabListOverflow? Overflow { get; set; }

    /// <summary>
    ///     Gets or sets the visual style of the tab list.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the style. The default is
    ///     <see cref="TabListVariant.Default" />.
    /// </value>
    [HtmlAttributeName("variant")]
    public TabListVariant? Variant { get; set; }

    /// <inheritdoc />
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var effectiveOrientation = Orientation ?? TabListOrientation.Horizontal;
        var effectiveVariant = Variant ?? TabListVariant.Default;
        var hasMenu =
            effectiveOrientation == TabListOrientation.Horizontal
            && (Overflow ?? TabListOverflow.Menu) == TabListOverflow.Menu;

        // sel-tab-list fits the tabs to the list's width
        output.TagName = hasMenu ? "sel-tab-list" : "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "tabs");
        output.Attributes.SetAttribute(
            "data-orientation",
            effectiveOrientation.GetDataAttributeText()
        );

        // The author's class belongs to the host only; the inner list carries its own classes.
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-tabs", output.GetUserSuppliedClass())
        );

        var tabListTagBuilder = new TagBuilder("div");
        tabListTagBuilder.Attributes.Add("data-slot", "tabs-list");
        tabListTagBuilder.Attributes.Add("data-variant", effectiveVariant.GetDataAttributeText());
        // Only tabs that switch panels form a tab list; link tabs keep link semantics.
        if (GetContext<TabsContext>(context) is not null)
        {
            tabListTagBuilder.Attributes.Add("role", "tablist");
            tabListTagBuilder.Attributes.Add(
                "aria-orientation",
                effectiveOrientation.GetDataAttributeText()
            );
        }
        tabListTagBuilder.Attributes.Add(
            "class",
            JoinCssClasses(
                "sa-tabs-list",
                effectiveVariant == TabListVariant.Default
                    ? "sa-tabs-list-variant-default"
                    : "sa-tabs-list-variant-line"
            )
        );
        tabListTagBuilder.InnerHtml.AppendHtml(await output.GetChildContentAsync());

        if (!hasMenu)
        {
            output.Content.AppendHtml(tabListTagBuilder);
            return;
        }

        var menuId = $"--sa-tab-list-{GetUniqueId(context)}-more";
        tabListTagBuilder.InnerHtml.AppendHtml(RenderMoreTrigger(context, menuId));
        output.Content.AppendHtml(tabListTagBuilder);

        // sel-tab-list fills the menu with the tabs it hides
        var menuTagBuilder = new TagBuilder("sel-dropdown-menu");
        menuTagBuilder.Attributes.Add("id", menuId);
        menuTagBuilder.Attributes.Add("popover", "");
        menuTagBuilder.Attributes.Add("role", "menu");
        menuTagBuilder.Attributes.Add("data-slot", "dropdown-menu-content");
        menuTagBuilder.Attributes.Add("data-anchor-side", "bottom");
        menuTagBuilder.Attributes.Add("data-side", "bottom");
        menuTagBuilder.Attributes.Add(
            "class",
            JoinCssClasses(
                "sa-dropdown-menu-content",
                "sa-dropdown-menu-content-logical",
                AnchorPlacement.GetClassName("bottom", "start")
            )
        );
        output.Content.AppendHtml(menuTagBuilder);
    }

    // Hidden until sel-tab-list hides a tab
    private IHtmlContent RenderMoreTrigger(TagHelperContext context, string menuId)
    {
        var triggerTagBuilder = new TagBuilder("button");
        triggerTagBuilder.Attributes.Add("type", "button");
        triggerTagBuilder.Attributes.Add("popovertarget", menuId);
        triggerTagBuilder.Attributes.Add("aria-haspopup", "menu");
        triggerTagBuilder.Attributes.Add("data-slot", "tabs-more");
        triggerTagBuilder.Attributes.Add("class", "sa-tabs-trigger sa-tabs-more");
        triggerTagBuilder.Attributes.Add("hidden", "");
        triggerTagBuilder.InnerHtml.Append(MoreLabel ?? "More");

        var iconOutput = new TagHelperOutput(
            string.Empty,
            [new TagHelperAttribute("data-icon", "inline-end")],
            (_, _) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent())
        );
        new IconTagHelper(_iconOptions)
        {
            Name = _iconOptions.GetSemanticIconName(SemanticIconRole.DropdownIndicator),
        }.Process(context, iconOutput);
        triggerTagBuilder.InnerHtml.AppendHtml(iconOutput);

        return triggerTagBuilder;
    }
}
