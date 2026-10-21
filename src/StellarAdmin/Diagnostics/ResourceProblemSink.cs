namespace StellarAdmin.Diagnostics;

// Collects the problems that resource builders find while a resource's options are built. The nested builders only
// see the column, field or form they configure, not the resource's options, so they report here. The resource
// options factory opens the sink around its Configure and PostConfigure callbacks, which run synchronously on one
// thread. Outside a build, such as an editor configured in a test, reports are dropped.
internal static class ResourceProblemSink
{
    [ThreadStatic]
    private static List<ConfigurationProblem>? _problems;

    public static void Report(
        ConfigurationProblemKind kind,
        ConfigurationProblemEffect effect,
        string message,
        string outcome
    )
    {
        _problems?.Add(new(kind, effect, message, outcome));
    }

    // An editor or display setting set to a number it can't use keeps its current value
    public static void ReportInvalidNumber(string setting, int value, string requirement) =>
        Report(
            ConfigurationProblemKind.InvalidValue,
            ConfigurationProblemEffect.Ignored,
            $"{setting} is set to {value}, but it must be {requirement}.",
            $"{setting} keeps its default."
        );

    // Starts collecting for one resource, and returns the sink of an outer build to restore, if any
    public static List<ConfigurationProblem>? Open()
    {
        var outer = _problems;
        _problems = [];

        return outer;
    }

    // Returns the problems reported since Open, and restores the outer build's sink
    public static List<ConfigurationProblem> Close(List<ConfigurationProblem>? outer)
    {
        var problems = _problems ?? [];
        _problems = outer;

        return problems;
    }
}
