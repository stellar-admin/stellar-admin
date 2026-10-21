using StellarAdmin.Diagnostics;

namespace StellarAdmin.Resources.Options;

// The index page's paging, scopes and default sort, repaired to values the list can use
internal static class ResourceIndexCheck
{
    public static void Run<TResource>(ResourceOptions<TResource> options)
    {
        CheckPaging(options);
        CheckScopes(options);
        CheckDefaultSort(options);
    }

    private static void CheckPaging<TResource>(ResourceOptions<TResource> options)
    {
        if (options.Index.Paging is not { } paging)
        {
            return;
        }

        var sizes = paging.PageSizes.Where(size => size > 0).Distinct().ToList();
        if (sizes.Count != paging.PageSizes.Count || sizes.Count == 0)
        {
            var defaults = new ResourcePagingOptions();
            if (sizes.Count == 0)
            {
                sizes = [.. defaults.PageSizes];
            }

            options.Report(
                ConfigurationProblemKind.InvalidPageSizes,
                $"The list offers the page sizes {Format(sizes)} because the `PageSizes` {Format(paging.PageSizes)} aren't all positive and distinct.",
                "Set `PageSizes` to distinct numbers greater than 0.",
                paging.PageSizesSource
            );
        }

        var pageSize = paging.PageSize;
        if (pageSize <= 0)
        {
            options.Report(
                ConfigurationProblemKind.InvalidDefaultPageSize,
                $"The list shows {sizes[0]} rows per page because the page size `{pageSize}` isn't greater than 0.",
                "Set `PageSize` to one of the `PageSizes`.",
                paging.PageSizeSource
            );
            pageSize = sizes[0];
        }
        else if (!sizes.Contains(pageSize))
        {
            sizes.Add(pageSize);
            sizes.Sort();
            var problem = new ConfigurationProblem(
                ConfigurationProblemKind.InvalidDefaultPageSize,
                $"The page size `{pageSize}` isn't one of the `PageSizes`, so the list adds it to them.",
                $"Add `{pageSize}` to `PageSizes`, or set `PageSize` to one of them."
            )
            {
                Level = ConfigurationProblemLevel.Info,
            };
            var source = paging.PageSizeSource ?? paging.PageSizesSource;
            options.Problems.Add(source is null ? problem : problem with { Source = source });
        }

        paging.PageSizes = sizes;
        paging.PageSize = pageSize;
    }

    private static void CheckScopes<TResource>(ResourceOptions<TResource> options)
    {
        if (options.Index.Scopes is not { } scopes)
        {
            return;
        }

        var ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var scope in scopes.Items.ToArray())
        {
            if (string.IsNullOrWhiteSpace(scope.Id) || string.IsNullOrWhiteSpace(scope.Title))
            {
                scopes.MutableItems.Remove(scope);
                options.Report(
                    ConfigurationProblemKind.InvalidScope,
                    string.IsNullOrWhiteSpace(scope.Id)
                        ? $"The scope `{scope.Title}` isn't in the list's scopes because its identifier is blank."
                        : $"The scope `{scope.Id}` isn't in the list's scopes because its title is blank.",
                    string.IsNullOrWhiteSpace(scope.Id)
                        ? "Give the scope an identifier."
                        : "Give the scope a title.",
                    scope.Source
                );
            }
            else if (!ids.Add(scope.Id))
            {
                scopes.MutableItems.Remove(scope);
                options.Report(
                    ConfigurationProblemKind.RepeatedScope,
                    $"The scope `{scope.Title}` isn't in the list's scopes because an earlier scope already uses the identifier `{scope.Id}`.",
                    "Give each scope its own identifier.",
                    scope.Source
                );
            }
        }

        if (
            scopes.DefaultScope is { } defaultScope
            && scopes.Items.All(scope => scope.Id != defaultScope)
        )
        {
            scopes.DefaultScope = null;
            options.Report(
                ConfigurationProblemKind.UnknownDefaultScope,
                $"The list has no default scope because no scope has the identifier `{defaultScope}`.",
                "Set the default scope to the identifier of one of the scopes.",
                scopes.DefaultScopeSource
            );
        }
    }

    private static void CheckDefaultSort<TResource>(ResourceOptions<TResource> options)
    {
        var index = options.Index;
        if (
            index.DefaultSort is { } sort
            && !index.Columns.Any(column => column.Sortable && column.SortField == sort.Field)
        )
        {
            index.DefaultSort = null;
            var hasColumn = index.Columns.Any(column => column.SortField == sort.Field);
            options.Report(
                ConfigurationProblemKind.UnsortableDefaultSort,
                hasColumn
                    ? $"The list isn't sorted by `{sort.Field}` because its column isn't sortable. It uses the data source's order instead."
                    : $"The list isn't sorted by `{sort.Field}` because it has no column for it. It uses the data source's order instead.",
                hasColumn
                    ? $"Make the `{sort.Field}` column sortable, or sort by another column."
                    : $"Add a sortable `{sort.Field}` column, or sort by another column.",
                index.DefaultSortSource
            );
        }
    }

    private static string Format(IEnumerable<int> sizes) => string.Join(", ", sizes);
}
