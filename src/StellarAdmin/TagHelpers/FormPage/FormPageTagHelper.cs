using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Razor.TagHelpers;
using StellarAdmin.Areas.StellarAdmin.ViewModels;
using StellarAdmin.UI;

namespace StellarAdmin;

/// <summary>
///     Renders the standard form page of a resource.
/// </summary>
[HtmlTargetElement("sa-form-page", TagStructure = TagStructure.NormalOrSelfClosing)]
public class FormPageTagHelper : StellarAdminTemplatedTagHelperBase
{
    /// <summary>
    ///     Gets or sets the model of the form page to render.
    /// </summary>
    [HtmlAttributeName("model")]
    public required ResourceFormPageViewModel Model { get; set; }

    /// <inheritdoc />
    protected override string ViewName => "_FormPage";

    /// <summary>
    ///     Initializes a new instance of the <see cref="FormPageTagHelper" /> class.
    /// </summary>
    /// <param name="viewEngine">The view engine that finds the page's partial view.</param>
    public FormPageTagHelper(ICompositeViewEngine viewEngine)
        : base(viewEngine) { }

    /// <inheritdoc />
    protected override object? GetViewModel()
    {
        return Model;
    }
}
