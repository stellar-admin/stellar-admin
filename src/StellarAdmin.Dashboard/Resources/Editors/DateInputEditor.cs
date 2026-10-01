using System.Globalization;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace StellarAdmin.Dashboard.Resources.Editors;

/// <summary>
///     Configures a date input editor.
/// </summary>
public sealed class DateInputEditor
    : TemporalInputEditor<DateOnly>,
        IFieldEditor<DateInputEditorHandler>
{
    /// <summary>
    ///     The number of days between accepted dates, or null to accept every day.
    /// </summary>
    public int? Step { get; set; }

    internal override TemporalInputAttributes ResolveAttributes(ModelMetadata metadata) =>
        new(
            "date",
            "{0:yyyy-MM-dd}",
            Step?.ToString(CultureInfo.InvariantCulture),
            Min is { } min ? Format(min, "yyyy-MM-dd") : null,
            Max is { } max ? Format(max, "yyyy-MM-dd") : null
        );
}
