namespace StellarAdmin.Dashboard.Resources.Options;

/// <summary>
///     A field or layout container in a form.
/// </summary>
public abstract class FormItemOptions
{
    internal FormColumnSpanDefinitions ColumnSpan { get; set; }

    private protected FormItemOptions() { }
}
