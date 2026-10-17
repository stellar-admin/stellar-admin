using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     A progress bar that visualizes the completion of a task as a filled track. Compose
///     it with the label and value subcomponents.
/// </summary>
[HtmlTargetElement("sa-progress")]
public class ProgressTagHelper : StellarAdminTagHelperBase
{
    /// <summary>
    ///     The value representing full completion.
    /// </summary>
    /// <remarks>
    ///     Defaults to <c>100</c>.
    /// </remarks>
    [HtmlAttributeName("max")]
    public int? Max { get; set; }

    /// <summary>
    ///     The value representing no completion.
    /// </summary>
    /// <remarks>
    ///     Defaults to <c>0</c>.
    /// </remarks>
    [HtmlAttributeName("min")]
    public int? Min { get; set; }

    /// <summary>
    ///     The current progress value, between the minimum and maximum.
    /// </summary>
    /// <remarks>
    ///     Defaults to <c>0</c>.
    /// </remarks>
    [HtmlAttributeName("value")]
    public int? Value { get; set; }

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

    private int GetPercentageCompleted(int minimum, int maximum, int value)
    {
        return (int)Math.Round(((double)(value - minimum) / (maximum - minimum)) * 100);
    }
}
