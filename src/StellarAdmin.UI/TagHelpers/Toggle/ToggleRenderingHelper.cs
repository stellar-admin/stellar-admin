namespace StellarAdmin.UI;

internal static class ToggleRenderingHelper
{
    public static string BuildClass(bool includeGroupItemToken, string? userClass)
    {
        var elements = new List<string?> { "sa-toggle" };

        if (includeGroupItemToken)
        {
            elements.Add("sa-toggle-group-item");
        }

        // User-supplied class goes last so authoring overrides win in the merge.
        elements.Add(userClass);

        return StellarAdminTagHelperBase.JoinCssClasses(elements.ToArray()) ?? string.Empty;
    }
}
