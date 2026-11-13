using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Options;
using StellarAdmin.UI.Icons;

namespace StellarAdmin.UI;

/// <summary>
///     Renders a spinning icon that shows that something is loading.
/// </summary>
/// <remarks>
///     Screen readers announce the spinner as "Loading".
/// </remarks>
[HtmlTargetElement("sa-spinner")]
public sealed class SpinnerTagHelper : StellarAdminTagHelperBase
{
    private readonly IconOptions _iconOptions;

    /// <summary>
    ///     Initializes a new instance of the <see cref="SpinnerTagHelper" /> class.
    /// </summary>
    /// <param name="iconOptions">The icon options that supply the loading icon.</param>
    public SpinnerTagHelper(IOptions<IconOptions> iconOptions)
    {
        _iconOptions = iconOptions.Value;
    }

    /// <inheritdoc />
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        _iconOptions.RenderIcon(output, _iconOptions.GetSemanticIconName(SemanticIconRole.Loading));

        output.Attributes.SetAttribute("role", "status");
        output.Attributes.SetAttribute("aria-label", "Loading");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-spinner", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
