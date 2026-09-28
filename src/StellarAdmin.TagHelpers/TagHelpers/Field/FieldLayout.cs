namespace StellarAdmin.TagHelpers;

/// <summary>
///     How an automatically rendered field arranges its parts around the control.
/// </summary>
/// <remarks>
///     Parts render in the listed order, and parts that are not listed are not rendered. In a
///     field that is not vertical, the parts on each side of the control are grouped in a field
///     content container.
/// </remarks>
public sealed class FieldLayout
{
    /// <summary>
    ///     The control first, followed by the label, description, and error, arranged horizontally.
    /// </summary>
    public static FieldLayout ControlFirst { get; } =
        new(
            FieldOrientation.Horizontal,
            [],
            [FieldPart.Label, FieldPart.Description, FieldPart.Error]
        );

    /// <summary>
    ///     The label, control, error, and description, stacked vertically.
    /// </summary>
    public static FieldLayout Stacked { get; } =
        new(FieldOrientation.Vertical, [FieldPart.Label], [FieldPart.Error, FieldPart.Description]);

    /// <summary>
    ///     The label, description, control, and error, stacked vertically.
    /// </summary>
    public static FieldLayout StackedDescriptionFirst { get; } =
        new(FieldOrientation.Vertical, [FieldPart.Label, FieldPart.Description], [FieldPart.Error]);

    /// <summary>
    ///     The parts rendered after the control, in order.
    /// </summary>
    public IReadOnlyList<FieldPart> AfterControl { get; }

    /// <summary>
    ///     The parts rendered before the control, in order.
    /// </summary>
    public IReadOnlyList<FieldPart> BeforeControl { get; }

    /// <summary>
    ///     The orientation of the field.
    /// </summary>
    public FieldOrientation Orientation { get; }

    /// <summary>
    ///     Initializes a new instance of the <see cref="FieldLayout" /> class.
    /// </summary>
    /// <param name="orientation">The orientation of the field.</param>
    /// <param name="beforeControl">The parts rendered before the control, in order.</param>
    /// <param name="afterControl">The parts rendered after the control, in order.</param>
    /// <exception cref="ArgumentException">A part is listed more than once.</exception>
    public FieldLayout(
        FieldOrientation orientation,
        IEnumerable<FieldPart> beforeControl,
        IEnumerable<FieldPart> afterControl
    )
    {
        ArgumentNullException.ThrowIfNull(beforeControl);
        ArgumentNullException.ThrowIfNull(afterControl);

        FieldPart[] before = [.. beforeControl];
        FieldPart[] after = [.. afterControl];
        if (before.Concat(after).CountBy(part => part).Any(group => group.Value > 1))
        {
            throw new ArgumentException("A field part can be listed only once.");
        }

        Orientation = orientation;
        BeforeControl = before;
        AfterControl = after;
    }

    /// <summary>
    ///     Returns a copy of the layout that does not render the specified part.
    /// </summary>
    /// <param name="part">The part to remove.</param>
    public FieldLayout Without(FieldPart part)
    {
        return new FieldLayout(
            Orientation,
            BeforeControl.Where(p => p != part),
            AfterControl.Where(p => p != part)
        );
    }
}
