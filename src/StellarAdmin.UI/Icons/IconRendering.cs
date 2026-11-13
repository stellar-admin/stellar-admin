using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI.Icons;

// Renders registered icons as <svg> elements, for <sa-icon> and the icons components draw themselves
internal static class IconRendering
{
    private static readonly IconDefinition NotFoundIcon = new(
        LucideIcons.SvgAttributes,
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

    // The icon mapped to the role, with the attributes before the icon's own
    public static TagHelperOutput RenderIcon(
        this IconOptions iconOptions,
        SemanticIconRole role,
        params (string Name, string Value)[] attributes
    )
    {
        return iconOptions.RenderIcon(iconOptions.GetSemanticIconName(role), attributes);
    }

    // The named icon, or a placeholder when no icon has the name, with the attributes before the
    // icon's own
    public static TagHelperOutput RenderIcon(
        this IconOptions iconOptions,
        string? name,
        params (string Name, string Value)[] attributes
    )
    {
        var output = new TagHelperOutput(
            "svg",
            new TagHelperAttributeList(
                attributes.Select(attribute => new TagHelperAttribute(
                    attribute.Name,
                    attribute.Value
                ))
            ),
            (_, _) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent())
        );
        iconOptions.RenderIcon(output, name);

        return output;
    }

    // Turns the output into the icon's <svg>; attributes already on the output win over the icon's
    public static void RenderIcon(
        this IconOptions iconOptions,
        TagHelperOutput output,
        string? name
    )
    {
        var iconDefinition =
            name != null && iconOptions.TryGetIcon(name, out var foundIcon)
                ? foundIcon
                : NotFoundIcon;

        output.TagName = "svg";
        output.TagMode = TagMode.StartTagAndEndTag;

        foreach (var (attributeName, value) in iconDefinition.Attributes)
        {
            if (!output.Attributes.ContainsName(attributeName))
            {
                output.Attributes.SetAttribute(attributeName, value);
            }
        }

        foreach (var shape in iconDefinition.Shapes)
        {
            var shapeBuilder = new TagBuilder(shape.Name);
            foreach (var (attributeName, attributeValue) in shape.Attributes)
            {
                shapeBuilder.Attributes.Add(attributeName, attributeValue);
            }

            output.Content.AppendHtml(shapeBuilder);
        }
    }
}
