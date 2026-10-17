using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace StellarAdmin.Resources.Editors;

/// <summary>
///     Configures a time of day input editor.
/// </summary>
public sealed class TimeInputEditor
    : TemporalInputEditor<TimeOnly>,
        IFieldEditor<TimeInputEditorHandler>
{
    /// <summary>
    ///     Gets or sets the interval between accepted times.
    /// </summary>
    /// <value>
    ///     The interval, or <see langword="null" /> to accept any time. The default is
    ///     <see langword="null" />.
    /// </value>
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
