using Microsoft.AspNetCore.Mvc.Rendering;

namespace DataGridSpike;

public static class SpikeHtmlExtensions
{
    public const string ItemKey = "spike-grid-item";

    /// <summary>Reads the ambient row item set by the spike grid for the current row pass.</summary>
    public static string? SpikeItem(this IHtmlHelper html)
    {
        return html.ViewData[ItemKey] as string;
    }

    /// <summary>Typed variant for object row items.</summary>
    public static T? SpikeItem<T>(this IHtmlHelper html)
        where T : class
    {
        return html.ViewData[ItemKey] as T;
    }
}
