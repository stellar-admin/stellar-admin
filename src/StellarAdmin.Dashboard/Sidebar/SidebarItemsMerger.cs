namespace StellarAdmin.Dashboard.Sidebar;

// Merges the items of every ISidebarItemsProvider, so providers share groups. A merged group sits
// where its label first appears. Links within a group, and runs of consecutive ungrouped links,
// sort by Order; the stable sort keeps provider order, then position, for ties.
internal static class SidebarItemsMerger
{
    public static List<SidebarItem> Merge(IEnumerable<SidebarItem> items)
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
            merged[index] = (SidebarGroupItem)merged[index] with
            {
                Items = [.. groupItems.OrderBy(item => item.Order)],
            };
        }

        return SortUngroupedLinks(merged);
    }

    private static List<SidebarItem> SortUngroupedLinks(List<SidebarItem> items)
    {
        var sorted = new List<SidebarItem>(items.Count);
        var run = new List<SidebarLinkItemBase>();
        foreach (var item in items)
        {
            if (item is SidebarLinkItemBase link)
            {
                run.Add(link);
                continue;
            }

            sorted.AddRange(run.OrderBy(link => link.Order));
            run.Clear();
            sorted.Add(item);
        }

        sorted.AddRange(run.OrderBy(link => link.Order));

        return sorted;
    }
}
