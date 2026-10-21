namespace StellarAdmin.Resources.Options;

internal static class FormItems
{
    public static FormDisplayFieldOptions[] DisplayFields(IFormScope? scope) =>
        scope is null ? [] : ResourceFormOptions.EnumerateDisplayFields(scope.Items).ToArray();

    public static FormFieldOptions[] Fields(ResourceFormOptions? form) =>
        form?.Fields.ToArray() ?? [];

    // Removes a field from a form or view page, wherever its sections, groups or details hold it
    public static bool Remove(IFormScope scope, FormItemOptions item) => Remove(scope.Items, item);

    private static bool Remove(IList<FormItemOptions> items, FormItemOptions item)
    {
        if (items.Remove(item))
        {
            return true;
        }

        foreach (var child in items)
        {
            var removed = child switch
            {
                FormDetailsOptions details when item is FormDisplayFieldOptions field =>
                    details.MutableFields.Remove(field),
                IFormScope container => Remove(container.Items, item),
                _ => false,
            };
            if (removed)
            {
                return true;
            }
        }

        return false;
    }
}
