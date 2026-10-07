namespace StellarAdmin.Dashboard.Resources.Options;

internal static class FormGrid
{
    public const int MaxColumns = 12;

    public static int ValidateColumns(int value, string paramName)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(value, 1, paramName);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(value, MaxColumns, paramName);

        return value;
    }
}
