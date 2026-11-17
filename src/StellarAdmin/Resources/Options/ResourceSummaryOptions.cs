using StellarAdmin.Diagnostics;

namespace StellarAdmin.Resources.Options;

// The record summary under the title of the view and edit pages: fields, text and separators, in order
internal sealed class ResourceSummaryOptions
{
    public List<SummaryItem> Items { get; } = [];

    public bool SeparateItems { get; set; }

    public ConfigurationSource? SeparateItemsSource { get; set; }

    // The field items, whose values the page prepares and the data source loads
    public IEnumerable<FormDisplayFieldOptions> Fields =>
        Items.OfType<SummaryFieldItem>().Select(item => item.Field);

    // The items as the page renders them, with a separator between every two items when SeparateItems is set
    public IReadOnlyList<SummaryItem> GetRenderedItems()
    {
        if (!SeparateItems)
        {
            return Items;
        }

        var items = new List<SummaryItem>();
        foreach (var item in Items)
        {
            if (items.Count > 0)
            {
                items.Add(new SummarySeparatorItem(null));
            }

            items.Add(item);
        }

        return items;
    }
}

internal abstract record SummaryItem;

internal sealed record SummaryFieldItem(FormDisplayFieldOptions Field) : SummaryItem;

internal sealed record SummaryTextItem(string Text) : SummaryItem;

internal sealed record SummarySeparatorItem(ConfigurationSource? Source) : SummaryItem;
