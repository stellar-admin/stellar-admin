using StellarAdmin.Dashboard.Resources.Options;

namespace StellarAdmin.Dashboard.Resources.Builders;

/// <summary>
///     Configures the number of columns a form item spans at each breakpoint.
/// </summary>
/// <remarks>
///     A breakpoint without a span uses the span of the next smaller breakpoint. A span is
///     limited to the number of columns in its grid.
/// </remarks>
public sealed class ColumnSpanBuilder
{
    internal FormColumnSpanDefinitions Span { get; private set; }

    internal ColumnSpanBuilder() { }

    /// <summary>
    ///     Sets the span at the smallest width.
    /// </summary>
    /// <remarks>
    ///     Defaults to 1.
    /// </remarks>
    public ColumnSpanBuilder Default(int span)
    {
        Span = Span with { Default = FormGrid.ValidateColumns(span, nameof(span)) };

        return this;
    }

    /// <summary>
    ///     Spans all columns at the smallest width.
    /// </summary>
    public ColumnSpanBuilder DefaultFull()
    {
        Span = Span with { Default = FormColumnSpanDefinitions.Full };

        return this;
    }

    /// <summary>
    ///     Sets the span when the form is at least 56rem wide.
    /// </summary>
    public ColumnSpanBuilder Large(int span)
    {
        Span = Span with { Large = FormGrid.ValidateColumns(span, nameof(span)) };

        return this;
    }

    /// <summary>
    ///     Spans all columns when the form is at least 56rem wide.
    /// </summary>
    public ColumnSpanBuilder LargeFull()
    {
        Span = Span with { Large = FormColumnSpanDefinitions.Full };

        return this;
    }

    /// <summary>
    ///     Sets the span when the form is at least 40rem wide.
    /// </summary>
    public ColumnSpanBuilder Medium(int span)
    {
        Span = Span with { Medium = FormGrid.ValidateColumns(span, nameof(span)) };

        return this;
    }

    /// <summary>
    ///     Spans all columns when the form is at least 40rem wide.
    /// </summary>
    public ColumnSpanBuilder MediumFull()
    {
        Span = Span with { Medium = FormColumnSpanDefinitions.Full };

        return this;
    }

    /// <summary>
    ///     Sets the span when the form is at least 30rem wide.
    /// </summary>
    public ColumnSpanBuilder Small(int span)
    {
        Span = Span with { Small = FormGrid.ValidateColumns(span, nameof(span)) };

        return this;
    }

    /// <summary>
    ///     Spans all columns when the form is at least 30rem wide.
    /// </summary>
    public ColumnSpanBuilder SmallFull()
    {
        Span = Span with { Small = FormColumnSpanDefinitions.Full };

        return this;
    }

    internal static FormColumnSpanDefinitions Build(Action<ColumnSpanBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        var builder = new ColumnSpanBuilder();
        configure(builder);

        return builder.Span;
    }

    internal static FormColumnSpanDefinitions FromSpan(int span) =>
        new(Default: FormGrid.ValidateColumns(span, nameof(span)));
}
