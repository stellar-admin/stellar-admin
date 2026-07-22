using StellarAdmin.UI.Theming;

namespace StellarAdmin.UI;

internal class DefaultCssClassMerger : ICssClassMerger
{
    /// <summary>
    ///     Joins the elements into a class string, de-duplicating repeats. A theme token is
    ///     emitted as its own class name; the linked theme stylesheet carries the matching
    ///     <c>.sa-*</c> rule. Conflict resolution happens in CSS (cascade layers: author
    ///     utilities beat component rules), so no tailwind-merge pass is needed.
    /// </summary>
    public string? Merge(params ClassElement?[] classes)
    {
        var seen = new HashSet<string>(StringComparer.Ordinal);
        var result = new List<string>();

        foreach (var element in classes)
        {
            var value = element switch
            {
                ThemeToken token => token.Name,
                ClassList list => list.Classes,
                _ => null,
            };

            if (string.IsNullOrWhiteSpace(value))
            {
                continue;
            }

            foreach (var cssClass in value.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            {
                if (seen.Add(cssClass))
                {
                    result.Add(cssClass);
                }
            }
        }

        return result.Count == 0 ? null : string.Join(' ', result);
    }
}
