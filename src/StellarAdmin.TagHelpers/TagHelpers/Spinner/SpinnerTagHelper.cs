using Microsoft.AspNetCore.Razor.TagHelpers;
using StellarAdmin.TagHelpers.Icons;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     An animated spinning icon that indicates a loading or busy state.
/// </summary>
[HtmlTargetElement("sa-spinner")]
public class SpinnerTagHelper : StellarAdminTagHelperBase
{
    private readonly IIconManager _iconManager;

    public SpinnerTagHelper(IIconManager iconManager)
    {
        _iconManager = iconManager ?? throw new ArgumentNullException(nameof(iconManager));
    }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var iconTagHelper = new IconTagHelper(_iconManager) { Name = "loader-circle" };
        await iconTagHelper.ProcessAsync(context, output);

        output.Attributes.SetAttribute("role", "status");
        output.Attributes.SetAttribute("aria-label", "Loading");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-spinner", output.GetUserSuppliedClass())
        );
    }
}
