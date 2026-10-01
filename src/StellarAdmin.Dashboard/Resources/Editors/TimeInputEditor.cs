using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace StellarAdmin.Dashboard.Resources.Editors;

/// <summary>
///     Configures a time of day input editor.
/// </summary>
public sealed class TimeInputEditor
    : TemporalInputEditor<TimeOnly>,
        IFieldEditor<TimeInputEditorHandler>
{
    /// <summary>
    ///     The interval between accepted times, or null to accept any time.
    /// </summary>
    public TimeSpan? Step { get; set; }

    internal override TemporalInputAttributes ResolveAttributes(ModelMetadata metadata) =>
        new(
            "time",
            "{0:HH:mm:ss.fff}",
            Step is { } step ? FormatSeconds(step) : "any",
            Min is { } min ? Format(min, "HH:mm:ss") : null,
            Max is { } max ? Format(max, "HH:mm:ss") : null
        );
}
