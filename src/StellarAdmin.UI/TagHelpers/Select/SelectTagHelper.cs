using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Options;
using StellarAdmin.UI.Icons;
using FrameworkSelectTagHelper = Microsoft.AspNetCore.Mvc.TagHelpers.SelectTagHelper;

namespace StellarAdmin.UI;

/// <summary>
///     Renders a styled native <c>&lt;select&gt;</c> for choosing an option.
/// </summary>
[HtmlTargetElement("sa-select")]
public sealed class SelectTagHelper : FieldInputBaseTagHelper<SelectClassNames>
{
    private readonly IHtmlGenerator _htmlGenerator;
    private readonly IconOptions _iconOptions;
    private FrameworkSelectTagHelper? _frameworkTagHelper;

    /// <summary>
    ///     Initializes a new instance of the <see cref="SelectTagHelper" /> class.
    /// </summary>
    /// <param name="htmlGenerator">The generator that renders the field and the select.</param>
    /// <param name="iconOptions">The icon options that supply the dropdown icon.</param>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="htmlGenerator" /> is <see langword="null" />.
    /// </exception>
    public SelectTagHelper(IHtmlGenerator htmlGenerator, IOptions<IconOptions> iconOptions)
        : base(htmlGenerator)
    {
        _htmlGenerator = htmlGenerator ?? throw new ArgumentNullException(nameof(htmlGenerator));
        _iconOptions = iconOptions.Value;
    }

    /// <summary>
    ///     Gets or sets the options to render.
    /// </summary>
    /// <remarks>
    ///     Child option elements, if any, come before these options.
    /// </remarks>
    [HtmlAttributeName("asp-items")]
    public IEnumerable<SelectListItem>? Items { get; set; }

    /// <summary>
    ///     Gets or sets the size of the select.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the size. The default is
    ///     <see cref="SelectSize.Default" />.
    /// </value>
    [HtmlAttributeName("size")]
    public SelectSize? Size { get; set; }

    /// <inheritdoc />
    protected override void OnInit(TagHelperContext context)
    {
        if (For != null || Items != null)
        {
            _frameworkTagHelper = new FrameworkSelectTagHelper(_htmlGenerator)
            {
                For = For,
                Items = Items,
                ViewContext = ViewContext,
                Name = Name,
            };
            _frameworkTagHelper.Init(context);
        }
    }

    /// <inheritdoc />
    protected override async Task<FieldLayout> RenderInput(
        TagHelperContext context,
        TagHelperOutput output,
        IDictionary<string, object?>? htmlAttributes
    )
    {
        var effectiveSize = Size ?? SelectSize.Default;
        var userSuppliedClass = output.GetUserSuppliedClass();

        /*
         * Render the select
         */
        var selectOutput = new TagHelperOutput(
            "select",
            new TagHelperAttributeList(
                output
                    .Attributes.Where(a =>
                        !a.Name.Equals("class", StringComparison.OrdinalIgnoreCase)
                    )
                    .Union([
                        new TagHelperAttribute("data-slot", "native-select"),
                        new TagHelperAttribute("data-size", effectiveSize.GetDataAttributeText()),
                        new TagHelperAttribute("class", "sa-native-select"),
                    ])
            ),
            (_, _) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent())
        );
        if ((For != null || Items != null) && _frameworkTagHelper != null)
        {
            _frameworkTagHelper.Process(context, selectOutput);
            selectOutput.Content.SetHtmlContent(await output.GetChildContentAsync());
        }
        else
        {
            selectOutput.Content.SetHtmlContent(await output.GetChildContentAsync());
        }
        ApplyFieldAttributes(context, selectOutput.Attributes, FieldLayout.Stacked);
        ReadOnlyHandlers.ApplyToSelect(selectOutput.Attributes);
        output.Content.AppendHtml(selectOutput);

        /*
         * Render the icon
         */
        var iconTagHelperOutput = new TagHelperOutput(
            string.Empty,
            [
                new TagHelperAttribute("class", "sa-native-select-icon"),
                new TagHelperAttribute("aria-hidden", "true"),
                new TagHelperAttribute("data-slot", "native-select-icon"),
            ],
            (_, _) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent())
        );
        var iconTagHelper = new IconTagHelper(_iconOptions)
        {
            Name = _iconOptions.GetSemanticIconName(SemanticIconRole.DropdownIndicator),
        };
        await iconTagHelper.ProcessAsync(context, iconTagHelperOutput);

        output.Content.AppendHtml(iconTagHelperOutput);

        /*
         * Finally, render the wrapper. Everything the author wrote (id, name, disabled, aria-*)
         * has moved onto the <select>; the wrapper carries only its own attributes and class.
         */
        output.Attributes.Clear();
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "native-select-wrapper");
        output.Attributes.SetAttribute("data-size", effectiveSize.GetDataAttributeText());
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-native-select-wrapper", ClassNames?.Control, userSuppliedClass)
        );

        return FieldLayout.Stacked;
    }
}
