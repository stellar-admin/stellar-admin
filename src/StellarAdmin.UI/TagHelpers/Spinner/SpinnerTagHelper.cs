using Microsoft.AspNetCore.Razor.TagHelpers;
using StellarAdmin.UI.Icons;
using StellarAdmin.UI.Theming;

namespace StellarAdmin.UI.TagHelpers;

/// <summary>
///     An animated spinning icon that indicates a loading or busy state.
/// </summary>
[HtmlTargetElement("sa-spinner")]
public class SpinnerTagHelper : StellarAdminTagHelperBase
{
    private readonly ICssClassMerger _classMerger;
    private readonly IIconManager _iconManager;

    public SpinnerTagHelper(ICssClassMerger classMerger, IIconManager iconManager)
        : base(classMerger)
    {
        _classMerger = classMerger ?? throw new ArgumentNullException(nameof(classMerger));
        _iconManager = iconManager ?? throw new ArgumentNullException(nameof(iconManager));
    }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var iconTagHelper = new IconTagHelper(ClassMerger, _iconManager) { Name = "loader-circle" };
        await iconTagHelper.ProcessAsync(context, output);

        output.Attributes.SetAttribute("role", "status");
        output.Attributes.SetAttribute("aria-label", "Loading");
        output.Attributes.SetAttribute(
            "class",
            _classMerger.Merge(
                new ThemeToken("sa-spinner"),
                // size-4 stays literal: theme rules size icons via svg:not([class*='size-']) guards.
                "size-4",
                output.GetUserSuppliedClass()
            )
        );
    }
}
