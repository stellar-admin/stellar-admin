namespace StellarAdmin.Sidebar;

// Merges the items of every ISidebarItemsProvider, so providers share groups, then sorts the top
// level (groups and ungrouped links) and the items in each group. Items with an order come first,
// by order; the rest follow by label when sorting by label is on, then in the order they were
// added to the admin panel. Items from custom providers have no position, so they follow in
// provider order.
internal static class SidebarItemsMerger
{
    public static List<SidebarItem> Merge(IEnumerable<SidebarItem> items, SidebarOptions options)
    {
        var merged = new List<SidebarItem>();
        var groups = new Dictionary<string, (int Index, List<SidebarLinkItemBase> Items)>(
            StringComparer.Ordinal
        );
        foreach (var item in items)
        {
            if (item is not SidebarGroupItem group)
            {
                merged.Add(item);
            }
            else if (groups.TryGetValue(group.Label, out var existing))
            {
                existing.Items.AddRange(group.Items);
            }
            else
            {
                groups.Add(group.Label, (merged.Count, [.. group.Items]));
                merged.Add(group);
            }
        }

        foreach (var (index, groupItems) in groups.Values)
        {
            var label = merged[index].Label;
            var groupOptions = options.Groups.GetValueOrDefault(label);
            merged[index] = new SidebarGroupItem(
                label,
                Sort(
                    groupItems,
                    item => item.Order,
                    item => item.Position,
                    groupOptions?.SortItemsByLabel ?? options.SortItemsByLabel
                )
            );
        }

        return
        [
            .. Sort(
                merged,
                item =>
                    item is SidebarLinkItemBase link
                        ? link.Order
                        : options.Groups.GetValueOrDefault(item.Label)?.Order,
                item =>
                    item is SidebarGroupItem group
                        ? group.Items.Min(groupItem => groupItem.Position)
                        : ((SidebarLinkItemBase)item).Position,
                options.SortGroupsByLabel
            ),
        ];
    }

    // OrderBy is stable, so items that tie on everything keep the order they were merged in
    private static IEnumerable<T> Sort<T>(
        IEnumerable<T> items,
        Func<T, int?> order,
        Func<T, int?> position,
        bool byLabel
    )
        where T : SidebarItem
    {
        var sorted = items.OrderBy(item => order(item) is null).ThenBy(item => order(item));
        if (byLabel)
        {
            sorted = sorted.ThenBy(item => item.Label, StringComparer.CurrentCultureIgnoreCase);
        }

        return sorted.ThenBy(item => position(item) is null).ThenBy(item => position(item));
    }
}
