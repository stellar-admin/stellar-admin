using StellarAdmin.TagHelpers;

namespace StellarAdmin.Dashboard.Resources.Options;

internal static class FormGrid
{
    public static int ValidateColumns(int value, string paramName)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(value, 1, paramName);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(value, ResponsiveGrid.MaxColumns, paramName);

        return value;
    }
}
