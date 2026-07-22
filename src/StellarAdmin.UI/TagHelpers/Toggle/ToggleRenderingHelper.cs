namespace StellarAdmin.UI.TagHelpers;

internal static class ToggleRenderingHelper
{
    private static readonly Dictionary<ToggleVariant, string> VariantClasses = new Dictionary<
        ToggleVariant,
        string
    >
    {
        [ToggleVariant.Default] = "sa-toggle-variant-default",
        [ToggleVariant.Outline] = "sa-toggle-variant-outline",
    };

    private static readonly Dictionary<ToggleSize, string> SizeClasses = new Dictionary<
        ToggleSize,
        string
    >
    {
        [ToggleSize.Default] = "sa-toggle-size-default",
        [ToggleSize.Small] = "sa-toggle-size-sm",
        [ToggleSize.Large] = "sa-toggle-size-lg",
    };

    /*
     * The toggle visual is a <label> wrapping an sr-only native <input>. The checked/focus/
     * disabled state therefore lives on the inner input, which is why we read it through
     * has-[:checked] / has-[:focus-visible] / has-[:disabled] here (the sa-toggle token's own
     * checked/focus/validation styles were rewritten to the same has-* forms by the generator).
     */
    private const string BaseLayout = "group/toggle";

    /*
     * Group-item-only statics. A grouped toggle item raises the focused item above its
     * neighbours (focus-visible:z-10) so its ring/border isn't clipped by the adjacent item —
     * doubly needed here because joined groups overlap items with -space-x-px. The focus state
     * lives on the inner input, so this is the has-[:focus-visible] form (matching the ring).
     */
    public static string BuildClass(
        ICssClassMerger classMerger,
        ToggleVariant variant,
        ToggleSize size,
        bool includeGroupItemToken,
        string? userClass
    )
    {
        var elements = new List<string?>
        {
            "sa-toggle",
            BaseLayout,
            VariantClasses[variant],
            SizeClasses[size],
        };

        if (includeGroupItemToken)
        {
            elements.Add("sa-toggle-group-item");
        }

        // User-supplied class goes last so authoring overrides win in the merge.
        elements.Add(userClass);

        return classMerger.Merge(elements.ToArray()) ?? string.Empty;
    }
}
