using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Options;
using StellarAdmin.UI.Icons;

namespace StellarAdmin.UI;

/// <summary>
///     Renders a registered icon as an <c>&lt;svg&gt;</c> element.
/// </summary>
/// <remarks>
///     Attributes set on the element take precedence over the icon's own attributes.
/// </remarks>
[HtmlTargetElement("sa-icon", TagStructure = TagStructure.WithoutEndTag)]
[OutputElementHint("svg")]
public class IconTagHelper : StellarAdminTagHelperBase
{
    private readonly IconOptions _iconOptions;
    private static readonly IconDefinition NotFoundIcon = new IconDefinition(
        new Dictionary<string, string>
        {
            ["xmlns"] = "http://www.w3.org/2000/svg",
            ["width"] = "24",
            ["height"] = "24",
            ["viewBox"] = "0 0 24 24",
            ["fill"] = "none",
            ["stroke"] = "currentColor",
            ["stroke-width"] = "2",
            ["stroke-linecap"] = "round",
            ["stroke-linejoin"] = "round",
        },
        [
            new SvgShape(
                "path",
                new Dictionary<string, string>
                {
                    ["d"] =
                        "M6 22a2 2 0 0 1-2-2V4a2 2 0 0 1 2-2h8a2.4 2.4 0 0 1 1.704.706l3.588 3.588A2.4 2.4 0 0 1 20 8v12a2 2 0 0 1-2 2z",
                }
            ),
            new SvgShape("path", new Dictionary<string, string> { ["d"] = "M12 9v4" }),
            new SvgShape("path", new Dictionary<string, string> { ["d"] = "M12 17h.01" }),
        ]
    );

    /// <summary>
    ///     Initializes a new instance of the <see cref="IconTagHelper" /> class.
    /// </summary>
    /// <param name="iconOptions">The icon options that supply the icons.</param>
    public IconTagHelper(IOptions<IconOptions> iconOptions)
        : this(iconOptions.Value) { }

    internal IconTagHelper(IconOptions iconOptions)
    {
        _iconOptions = iconOptions;
    }

    /// <summary>
    ///     Gets or sets the name of the icon to render.
    /// </summary>
    /// <remarks>
    ///     A placeholder icon is rendered when no icon has the name.
    /// </remarks>
    [HtmlAttributeName("name")]
    public string? Name { get; set; }

    /// <inheritdoc />
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var iconDefinition =
            Name != null && _iconOptions.TryGetIcon(Name, out var foundIcon)
                ? foundIcon
                : NotFoundIcon;

        output.TagName = "svg";
        output.TagMode = TagMode.StartTagAndEndTag;

        foreach (var (name, value) in iconDefinition.Attributes)
            if (!output.Attributes.ContainsName(name))
                output.Attributes.SetAttribute(name, value);

        foreach (var shape in iconDefinition.Shapes)
        {
            var shapeBuilder = new TagBuilder(shape.Name);
            foreach (var (attributeName, attributeValue) in shape.Attributes)
                shapeBuilder.Attributes.Add(attributeName, attributeValue);

            output.Content.AppendHtml(shapeBuilder);
        }
    }
}
