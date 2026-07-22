namespace StellarAdmin.UI;

internal class DefaultCssClassMerger : ICssClassMerger
{
    /// <summary>
    ///     Joins the class strings, de-duplicating repeats. Component class names (<c>sa-*</c>)
    ///     are emitted as-is — the linked theme stylesheet carries the matching rules, and
    ///     conflict resolution happens in CSS (cascade layers: author utilities beat component
    ///     rules).
    /// </summary>
    public string? Merge(params string?[] classes)
    {
        var seen = new HashSet<string>(StringComparer.Ordinal);
        var result = new List<string>();

        foreach (var value in classes)
        {
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
