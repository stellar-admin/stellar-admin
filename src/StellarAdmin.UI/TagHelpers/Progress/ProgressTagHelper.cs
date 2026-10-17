using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders a progress bar that shows how far a task has progressed.
/// </summary>
/// <remarks>
///     Add a <c>&lt;sa-progress-label&gt;</c> and a <c>&lt;sa-progress-value&gt;</c> inside to
///     show a label and the value.
/// </remarks>
[HtmlTargetElement("sa-progress")]
public class ProgressTagHelper : StellarAdminTagHelperBase
{
    /// <summary>
    ///     Gets or sets the value at which the task is complete.
    /// </summary>
    /// <value>
    ///     A value greater than <see cref="Min" />. The default is <c>100</c>.
    /// </value>
    [HtmlAttributeName("max")]
    public double? Max { get; set; }

    /// <summary>
    ///     Gets or sets the value at which the task has not started.
    /// </summary>
    /// <value>
    ///     A value less than <see cref="Max" />. The default is <c>0</c>.
    /// </value>
    [HtmlAttributeName("min")]
    public double? Min { get; set; }

    /// <summary>
    ///     Gets or sets the current progress.
    /// </summary>
    /// <value>
    ///     A value from <see cref="Min" /> to <see cref="Max" />. The default is <c>0</c>.
    /// </value>
    [HtmlAttributeName("value")]
    public double? Value { get; set; }

    /// <inheritdoc />
    /// <exception cref="ArgumentOutOfRangeException">
    ///     <see cref="Min" /> isn't less than <see cref="Max" />, or <see cref="Value" /> is outside
    ///     that range.
    /// </exception>
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var effectiveMin = Min ?? 0;
        var effectiveMax = Max ?? 100;
        var effectiveValue = Value ?? 0;

        if (effectiveMin >= effectiveMax)
        {
            throw new ArgumentOutOfRangeException(nameof(Min), "Min must be less than Max");
        }

        if (effectiveValue < effectiveMin || effectiveValue > effectiveMax)
        {
            throw new ArgumentOutOfRangeException(nameof(Value), "Must be between Min and Max");
        }

        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "progress");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-progress-root", output.GetUserSuppliedClass())
        );

        var trackTagBuilder = new TagBuilder("div");
        trackTagBuilder.Attributes.Add("data-slot", "progress-track");
        trackTagBuilder.Attributes.Add("class", JoinCssClasses("sa-progress-track"));

        var indicatorTagBuilder = new TagBuilder("div");
        indicatorTagBuilder.Attributes.Add("data-slot", "progress-indicator");
        indicatorTagBuilder.Attributes.Add("class", JoinCssClasses("sa-progress-indicator"));
        indicatorTagBuilder.Attributes.Add(
            "style",
            $"inset-inline-start: 0px; height: inherit; width: {GetPercentageCompleted(effectiveMin, effectiveMax, effectiveValue)}%;"
        );
        trackTagBuilder.InnerHtml.AppendHtml(indicatorTagBuilder);
        output.PostContent.AppendHtml(trackTagBuilder);
    }

    private int GetPercentageCompleted(double minimum, double maximum, double value)
    {
        return (int)Math.Round((value - minimum) / (maximum - minimum) * 100);
    }
}
