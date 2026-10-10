namespace StellarAdmin.TagHelpers;

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

    public static string BuildClass(
        ToggleVariant variant,
        ToggleSize size,
        bool includeGroupItemToken,
        string? userClass
    )
    {
        var elements = new List<string?>
        {
            "sa-toggle",
            VariantClasses[variant],
            SizeClasses[size],
        };

        if (includeGroupItemToken)
        {
            elements.Add("sa-toggle-group-item");
        }

        // User-supplied class goes last so authoring overrides win in the merge.
        elements.Add(userClass);

        return StellarAdminTagHelperBase.JoinCssClasses(elements.ToArray()) ?? string.Empty;
    }
}
