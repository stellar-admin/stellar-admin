using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Razor.TagHelpers;
using StellarAdmin.Areas.StellarAdmin.ViewModels;
using StellarAdmin.UI;

namespace StellarAdmin;

/// <summary>
///     Renders the standard view page of a resource.
/// </summary>
[HtmlTargetElement("sa-view-page", TagStructure = TagStructure.NormalOrSelfClosing)]
public sealed class ViewPageTagHelper : StellarAdminTemplatedTagHelperBase
{
    /// <summary>
    ///     Gets or sets the model of the view page to render.
    /// </summary>
    [HtmlAttributeName("model")]
    public required ResourceViewPageViewModel Model { get; set; }

    /// <inheritdoc />
    protected override string ViewName => "_ViewPage";

    /// <summary>
    ///     Initializes a new instance of the <see cref="ViewPageTagHelper" /> class.
    /// </summary>
    /// <param name="viewEngine">The view engine that finds the page's partial view.</param>
    public ViewPageTagHelper(ICompositeViewEngine viewEngine)
        : base(viewEngine) { }

    /// <inheritdoc />
    protected override object? GetViewModel()
    {
        return Model;
    }
}
