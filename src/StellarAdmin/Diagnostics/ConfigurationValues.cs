namespace StellarAdmin.Diagnostics;

internal static class ConfigurationValues
{
    // Describes the first blank value, for a value object that its builder method reports and leaves out
    public static string? FindBlank(params (string Name, string Value)[] values)
    {
        foreach (var (name, value) in values)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return $"Its {name} is blank.";
            }
        }

        return null;
    }
}
