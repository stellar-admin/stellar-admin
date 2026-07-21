using Microsoft.AspNetCore.Razor.TagHelpers;
using StellarAdmin.UI.Icons;
using StellarAdmin.UI.Theming;

namespace StellarAdmin.UI.TagHelpers;

/// <summary>
///     A separator placed between <c>sa-input-otp-group</c>s. Renders a Lucide <c>minus</c> icon
///     by default; supply child content to override it.
/// </summary>
[HtmlTargetElement("sa-input-otp-separator")]
public class InputOtpSeparatorTagHelper : StellarAdminTagHelperBase
{
    private readonly IIconManager _iconManager;

    public InputOtpSeparatorTagHelper(ICssClassMerger classMerger, IIconManager iconManager)
        : base(classMerger)
    {
        _iconManager = iconManager ?? throw new ArgumentNullException(nameof(iconManager));
    }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var userClass = output.GetUserSuppliedClass();

        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.Attributes.SetAttribute("data-slot", "input-otp-separator");
        output.Attributes.SetAttribute("role", "separator");
        output.Attributes.SetAttribute(
            "class",
            InputOtpRenderer.SeparatorClass(ClassMerger, userClass)
        );

        var childContent = await output.GetChildContentAsync();
        if (!childContent.IsEmptyOrWhiteSpace)
        {
            output.Content.AppendHtml(childContent);
        }
        else
        {
            await InputOtpRenderer.RenderDefaultSeparatorContentAsync(
                output.Content,
                context,
                ClassMerger,
                _iconManager
            );
        }
    }
}
