using System.Diagnostics;
using System.Reflection;

namespace StellarAdmin.Diagnostics;

// The line of the app's code that configured something, so a configuration problem can point at it. It comes from
// the app's debug symbols, so it is missing where the app was built without them.
internal sealed record ConfigurationSource(string FilePath, int Line)
{
    private static readonly HashSet<string> OwnAssemblies =
    [
        "StellarAdmin",
        "StellarAdmin.UI",
        "StellarAdmin.EntityFrameworkCore",
    ];

    // The app's code that called into the admin panel. Only a caller right outside the admin panel's own code
    // counts: when that is framework code, such as the options framework running a deferred callback, nothing on
    // the stack configured it.
    public static ConfigurationSource? Capture()
    {
        foreach (var frame in new StackTrace(1, fNeedFileInfo: true).GetFrames())
        {
            if (IsOwn(frame.GetMethod()))
            {
                continue;
            }

            return FromFrame(frame);
        }

        return null;
    }

    // The app's code nearest to where the exception was thrown
    public static ConfigurationSource? FromException(Exception exception) =>
        new StackTrace(exception, fNeedFileInfo: true)
            .GetFrames()
            .Where(frame => !IsOwn(frame.GetMethod()))
            .Select(FromFrame)
            .FirstOrDefault(source => source is not null);

    // The path relative to the app's content root when the file is under it, for a shorter label
    public string Describe(string? contentRoot)
    {
        var path =
            contentRoot is not null
            && Path.GetRelativePath(contentRoot, FilePath) is var relative
            && !relative.StartsWith("..", StringComparison.Ordinal)
            && !Path.IsPathRooted(relative)
                ? relative
                : FilePath;

        return $"{path}:{Line}";
    }

    public override string ToString() => $"{FilePath}:{Line}";

    private static ConfigurationSource? FromFrame(StackFrame frame) =>
        frame.GetFileName() is { Length: > 0 } file && frame.GetFileLineNumber() is > 0 and var line
            ? new(file, line)
            : null;

    private static bool IsOwn(MethodBase? method) =>
        method?.DeclaringType?.Assembly.GetName().Name is { } name && OwnAssemblies.Contains(name);
}
