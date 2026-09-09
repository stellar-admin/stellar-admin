using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     A field input with typed CSS classes for its parts.
/// </summary>
/// <typeparam name="TClassNames">The supported CSS class properties.</typeparam>
public abstract class FieldInputBaseTagHelper<TClassNames> : FieldInputBaseTagHelper
    where TClassNames : FieldClassNames
{
    /// <summary>
    ///     Additional CSS classes for the field and control parts.
    /// </summary>
    [HtmlAttributeName("class-names")]
    public TClassNames? ClassNames { get; set; }

    protected override FieldClassNames? FieldClasses => ClassNames;

    protected FieldInputBaseTagHelper(IHtmlGenerator htmlGenerator)
        : base(htmlGenerator) { }
}
