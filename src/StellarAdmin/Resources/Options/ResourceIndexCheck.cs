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
                $"The page sizes {Format(paging.PageSizes)} must be positive and distinct.",
                $"The list offers the page sizes {Format(sizes)}.",
                paging.PageSizesSource
            );
        }

        var pageSize = paging.PageSize;
        if (pageSize <= 0)
        {
            options.Report(
                ConfigurationProblemKind.InvalidDefaultPageSize,
                $"The default page size {pageSize} must be positive.",
                $"The list shows {sizes[0]} rows per page.",
                paging.PageSizeSource
            );
            pageSize = sizes[0];
        }
        else if (!sizes.Contains(pageSize))
        {
            sizes.Add(pageSize);
            sizes.Sort();
            options.Report(
                ConfigurationProblemKind.InvalidDefaultPageSize,
                $"The default page size {pageSize} isn't one of the page sizes.",
                $"The list adds it to the page sizes: {Format(sizes)}.",
                paging.PageSizeSource ?? paging.PageSizesSource
            );
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
                        ? $"The scope '{scope.Title}' has a blank identifier. Give it an identifier."
                        : $"The scope '{scope.Id}' has a blank title. Give it a title.",
                    "The scope is left out.",
                    scope.Source
                );
            }
            else if (!ids.Add(scope.Id))
            {
                scopes.MutableItems.Remove(scope);
                options.Report(
                    ConfigurationProblemKind.RepeatedScope,
                    $"The scope identifier '{scope.Id}' is used more than once. Give each scope its own identifier.",
                    $"The later scope '{scope.Title}' is left out.",
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
                $"The default scope '{defaultScope}' matches no scope.",
                "The list has no default scope.",
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
            options.Report(
                ConfigurationProblemKind.UnsortableDefaultSort,
                $"The default sort '{sort.Field}' must select a configured sortable column.",
                "The list uses the data source's order.",
                index.DefaultSortSource
            );
        }
    }

    private static string Format(IEnumerable<int> sizes) => string.Join(", ", sizes);
}
