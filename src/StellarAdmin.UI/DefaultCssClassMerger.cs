using StellarAdmin.UI.Theming;
using TailwindMerge;

namespace StellarAdmin.UI;

internal class DefaultCssClassMerger : ICssClassMerger
{
    private readonly TwMerge _twMerge;

    public DefaultCssClassMerger(TwMerge twMerge)
    {
        _twMerge = twMerge ?? throw new ArgumentNullException(nameof(twMerge));
    }

    public string? Merge(params ClassElement?[] classes)
    {
        return _twMerge.Merge(
            classes
                .Select(c =>
                {
                    return c switch
                    {
                        // A theme token is emitted as its own class name; the linked theme
                        // stylesheet carries the matching .sa-* rule. TwMerge passes unknown
                        // classes through untouched, so tokens never conflict with utilities.
                        ThemeToken cn => cn.Name,
                        ClassList cl => cl.Classes,
                        _ => string.Empty,
                    };
                })
                .ToArray()
        );
    }
}
