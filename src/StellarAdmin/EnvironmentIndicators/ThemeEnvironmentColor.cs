namespace StellarAdmin;

internal sealed class ThemeEnvironmentColor(EnvironmentThemeColor value) : EnvironmentColor
{
    public EnvironmentThemeColor Value { get; } = value;
}

internal enum EnvironmentThemeColor
{
    Neutral,
    Info,
    Success,
    Warning,
    Danger,
}
