using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Options;
using StellarAdmin.UI.Icons;

namespace StellarAdmin.UI;

/// <summary>
///     Renders a registered icon as an <c>&lt;svg&gt;</c> element.
/// </summary>
/// <remarks>
///     <para>
///         Attributes set on the element take precedence over the icon's own attributes.
///     </para>
///     <para>
///         Inside an <c>&lt;sa-icon-picker&gt;</c>, the icon is one of the icons the picker
///         offers, and the picker places it.
///     </para>
/// </remarks>
[HtmlTargetElement("sa-icon", TagStructure = TagStructure.WithoutEndTag)]
[OutputElementHint("svg")]
public sealed class IconTagHelper : StellarAdminTagHelperBase
{
    private readonly IconOptions _iconOptions;

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
        var picker = GetContext<IconPickerContext>(context);
        if (picker is not null && ReferenceEquals(picker.Owner, ParentTagHelper))
        {
            var svg = new TagHelperOutput(
                "svg",
                new TagHelperAttributeList(output.Attributes),
                (_, _) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent())
            );
            _iconOptions.RenderIcon(svg, Name);
            picker.Icons.Add((Name, svg));
            output.SuppressOutput();

            return;
        }

        _iconOptions.RenderIcon(output, Name);
    }
}
