using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Razor.TagHelpers;
using StellarAdmin.Pro.Areas.StellarAdmin.ViewModels;
using StellarAdmin.TagHelpers;

namespace StellarAdmin.Pro.TagHelpers;

/// <summary>
///     Renders the standard form page for the current form view model.
/// </summary>
[HtmlTargetElement("sa-form-page", TagStructure = TagStructure.NormalOrSelfClosing)]
public class FormPageTagHelper : StellarAdminTemplatedTagHelperBase
{
    /// <summary>The form to render.</summary>
    [HtmlAttributeName("model")]
    public required ResourceFormPageViewModel Model { get; set; }

    protected override string ViewName => "_FormPage";

    public FormPageTagHelper(ICompositeViewEngine viewEngine)
        : base(viewEngine) { }

    protected override object? GetViewModel()
    {
        return Model;
    }
}
