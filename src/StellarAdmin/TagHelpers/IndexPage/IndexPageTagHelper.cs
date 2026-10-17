using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Razor.TagHelpers;
using StellarAdmin.Areas.StellarAdmin.ViewModels;
using StellarAdmin.UI;

namespace StellarAdmin;

/// <summary>
///     Renders the standard index page of a resource.
/// </summary>
[HtmlTargetElement("sa-index-page", TagStructure = TagStructure.NormalOrSelfClosing)]
public class IndexPageTagHelper : StellarAdminTemplatedTagHelperBase
{
    /// <summary>
    ///     Gets or sets the model of the index page to render.
    /// </summary>
    [HtmlAttributeName("model")]
    public required IResourceIndexPageViewModel Model { get; set; }

    /// <inheritdoc />
    protected override string ViewName => "_IndexPage";

    /// <summary>
    ///     Initializes a new instance of the <see cref="IndexPageTagHelper" /> class.
    /// </summary>
    /// <param name="viewEngine">The view engine that finds the page's partial view.</param>
    public IndexPageTagHelper(ICompositeViewEngine viewEngine)
        : base(viewEngine) { }

    /// <inheritdoc />
    protected override object? GetViewModel()
    {
        return Model;
    }
}
