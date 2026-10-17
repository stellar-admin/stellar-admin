using System.Globalization;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace StellarAdmin.Resources.Editors;

/// <summary>
///     Configures a date input editor.
/// </summary>
public sealed class DateInputEditor
    : TemporalInputEditor<DateOnly>,
        IFieldEditor<DateInputEditorHandler>
{
    /// <summary>
    ///     Gets or sets the number of days between accepted dates.
    /// </summary>
    /// <value>
    ///     The number of days, or <see langword="null" /> to accept every day. The default is
    ///     <see langword="null" />.
    /// </value>
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
