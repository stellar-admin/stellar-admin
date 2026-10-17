using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Provides the base class for form controls that take CSS classes for each of their parts.
/// </summary>
/// <typeparam name="TClassNames">The type that holds the CSS classes for each part.</typeparam>
public abstract class FieldInputBaseTagHelper<TClassNames> : FieldInputBaseTagHelper
    where TClassNames : FieldClassNames
{
    /// <summary>
    ///     Gets or sets the CSS classes added to the parts of the field and the control.
    /// </summary>
    [HtmlAttributeName("class-names")]
    public TClassNames? ClassNames { get; set; }

    /// <inheritdoc />
    protected override FieldClassNames? FieldClasses => ClassNames;

    /// <summary>
    ///     Initializes a new instance of the <see cref="FieldInputBaseTagHelper{TClassNames}" />
    ///     class.
    /// </summary>
    /// <param name="htmlGenerator">The generator that renders a bound label and error.</param>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="htmlGenerator" /> is <see langword="null" />.
    /// </exception>
    protected FieldInputBaseTagHelper(IHtmlGenerator htmlGenerator)
        : base(htmlGenerator) { }
}
