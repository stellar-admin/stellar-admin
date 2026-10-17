namespace StellarAdmin.Resources.Options;

/// <summary>
///     Represents a field or a container of fields in a form.
/// </summary>
public abstract class FormItemOptions
{
    internal FormColumnSpanDefinitions ColumnSpan { get; set; }

    internal FormColumnStartDefinitions ColumnStart { get; set; }

    private protected FormItemOptions() { }
}
