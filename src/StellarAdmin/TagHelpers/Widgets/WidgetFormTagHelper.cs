using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using StellarAdmin.Areas.StellarAdmin;
using StellarAdmin.Areas.StellarAdmin.ViewModels.Internal;

namespace StellarAdmin;

/// <summary>
///     Renders a form that loads the widget it's in again, with the form's values as the
///     widget's inputs.
/// </summary>
/// <remarks>
///     Use it in a widget's partial view for controls that change what the widget shows, such as
///     a period picker. The form submits when a control changes. Name each control after a
///     property of the widget's parameters that has <c>[WidgetInput]</c>; other values are
///     ignored. Outside a widget with a handler, the form renders without its request.
/// </remarks>
[HtmlTargetElement("sa-widget-form")]
public sealed class WidgetFormTagHelper : TagHelper
{
    /// <summary>
    ///     Gets or sets the context of the view that renders the form.
    /// </summary>
    [ViewContext]
    [HtmlAttributeNotBound]
    public ViewContext ViewContext { get; set; } = null!;

    /// <inheritdoc />
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "form";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.Attributes.SetAttribute("data-slot", "widget-form");
        if (ViewContext.ViewData[ViewDataKeys.Widget] is not WidgetViewModel { Url: { } url })
        {
            return;
        }

        output.Attributes.SetAttribute("hx-get", url);
        output.Attributes.SetAttribute("hx-trigger", "change, submit");
        output.Attributes.SetAttribute("hx-target", "closest [data-slot='widget']");
        output.Attributes.SetAttribute("hx-swap", "outerHTML");
        output.Attributes.SetAttribute("hx-select", "");
        output.Attributes.SetAttribute("hx-select-oob", "");
        output.Attributes.SetAttribute("hx-push-url", "false");
    }
}
