using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Options;
using StellarAdmin.Icons;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     An animated spinning icon that indicates a loading or busy state.
/// </summary>
[HtmlTargetElement("sa-spinner")]
public class SpinnerTagHelper : StellarAdminTagHelperBase
{
    private readonly IconOptions _iconOptions;

    public SpinnerTagHelper(IOptions<IconOptions> iconOptions)
    {
        _iconOptions = iconOptions.Value;
    }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var iconTagHelper = new IconTagHelper(_iconOptions) { Name = "loader-circle" };
        await iconTagHelper.ProcessAsync(context, output);

        output.Attributes.SetAttribute("role", "status");
        output.Attributes.SetAttribute("aria-label", "Loading");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-spinner", output.GetUserSuppliedClass())
        );
    }
}
