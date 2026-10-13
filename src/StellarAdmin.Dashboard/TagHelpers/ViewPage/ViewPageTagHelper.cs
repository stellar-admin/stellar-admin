using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Razor.TagHelpers;
using StellarAdmin.Dashboard.Areas.StellarAdmin.ViewModels;
using StellarAdmin.TagHelpers;

namespace StellarAdmin.Dashboard.TagHelpers;

/// <summary>
///     Renders the standard view page for the current view page model.
/// </summary>
[HtmlTargetElement("sa-view-page", TagStructure = TagStructure.NormalOrSelfClosing)]
public class ViewPageTagHelper : StellarAdminTemplatedTagHelperBase
{
    /// <summary>The page to render.</summary>
    [HtmlAttributeName("model")]
    public required ResourceViewPageViewModel Model { get; set; }

    protected override string ViewName => "_ViewPage";

    public ViewPageTagHelper(ICompositeViewEngine viewEngine)
        : base(viewEngine) { }

    protected override object? GetViewModel()
    {
        return Model;
    }
}
