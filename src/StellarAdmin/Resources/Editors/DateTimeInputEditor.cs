using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace StellarAdmin.Resources.Editors;

/// <summary>
///     Configures a local date and time input editor.
/// </summary>
/// <remarks>
///     A <see cref="DateTimeOffset" /> property renders a text input in the round-trip format, so
///     its offset is kept. That input ignores <see cref="Step" />,
///     <see cref="TemporalInputEditor{TValue}.Min" /> and
///     <see cref="TemporalInputEditor{TValue}.Max" />.
/// </remarks>
public sealed class DateTimeInputEditor
    : TemporalInputEditor<DateTime>,
        IFieldEditor<DateTimeInputEditorHandler>
{
    /// <summary>
    ///     Gets or sets the interval between accepted times.
    /// </summary>
    /// <value>
    ///     The interval, or <see langword="null" /> to accept any time. The default is
    ///     <see langword="null" />.
    /// </value>
    public TimeSpan? Step { get; set; }

    internal override TemporalInputAttributes ResolveAttributes(ModelMetadata metadata)
    {
        if (metadata.UnderlyingOrModelType == typeof(DateTimeOffset))
        {
            return new("text", "{0:O}", null, null, null);
        }

        return new(
            "datetime-local",
            "{0:yyyy-MM-ddTHH:mm:ss.fff}",
            Step is { } step ? FormatSeconds(step) : "any",
            Min is { } min ? Format(min, "yyyy-MM-ddTHH:mm:ss") : null,
            Max is { } max ? Format(max, "yyyy-MM-ddTHH:mm:ss") : null
        );
    }
}
