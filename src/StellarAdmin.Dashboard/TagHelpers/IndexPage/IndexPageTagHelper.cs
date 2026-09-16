using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Razor.TagHelpers;
using StellarAdmin.Dashboard.Areas.StellarAdmin.ViewModels.Internal;
using StellarAdmin.TagHelpers;

namespace StellarAdmin.Dashboard.TagHelpers;

/// <summary>
///     Renders the standard index page for the current index view model.
/// </summary>
[HtmlTargetElement("sa-index-page", TagStructure = TagStructure.NormalOrSelfClosing)]
public class IndexPageTagHelper : StellarAdminTemplatedTagHelperBase
{
    /// <summary>The index to render.</summary>
    [HtmlAttributeName("model")]
    public required IResourceIndexPageViewModel Model { get; set; }

    protected override string ViewName => "_IndexPage";

    public IndexPageTagHelper(ICompositeViewEngine viewEngine)
        : base(viewEngine) { }

    protected override object? GetViewModel()
    {
        return Model;
    }
}
