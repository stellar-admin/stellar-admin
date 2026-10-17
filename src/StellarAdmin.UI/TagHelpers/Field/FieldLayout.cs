namespace StellarAdmin.UI;

/// <summary>
///     Specifies how a control's field arranges its parts around the control.
/// </summary>
/// <remarks>
///     Parts render in the listed order, and parts that aren't listed aren't rendered. In a
///     horizontal field, the parts on each side of the control are stacked together.
/// </remarks>
public sealed class FieldLayout
{
    /// <summary>
    ///     Gets a horizontal layout with the control first, followed by the label, description and
    ///     error.
    /// </summary>
    public static FieldLayout ControlFirst { get; } =
        new(
            FieldOrientation.Horizontal,
            [],
            [FieldPart.Label, FieldPart.Description, FieldPart.Error]
        );

    /// <summary>
    ///     Gets a vertical layout with the label, control, error and description.
    /// </summary>
    public static FieldLayout Stacked { get; } =
        new(FieldOrientation.Vertical, [FieldPart.Label], [FieldPart.Error, FieldPart.Description]);

    /// <summary>
    ///     Gets a vertical layout with the label, description, control and error.
    /// </summary>
    public static FieldLayout StackedDescriptionFirst { get; } =
        new(FieldOrientation.Vertical, [FieldPart.Label, FieldPart.Description], [FieldPart.Error]);

    /// <summary>
    ///     Gets the parts rendered after the control, in order.
    /// </summary>
    public IReadOnlyList<FieldPart> AfterControl { get; }

    /// <summary>
    ///     Gets the parts rendered before the control, in order.
    /// </summary>
    public IReadOnlyList<FieldPart> BeforeControl { get; }

    /// <summary>
    ///     Gets the orientation of the field.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies whether the parts are stacked or side by
    ///     side.
    /// </value>
    public FieldOrientation Orientation { get; }

    /// <summary>
    ///     Initializes a new instance of the <see cref="FieldLayout" /> class.
    /// </summary>
    /// <param name="orientation">
    ///     One of the enumeration values that specifies the orientation of the field.
    /// </param>
    /// <param name="beforeControl">The parts to render before the control, in order.</param>
    /// <param name="afterControl">The parts to render after the control, in order.</param>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="beforeControl" /> or <paramref name="afterControl" /> is
    ///     <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     A part is listed more than once.
    /// </exception>
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
    ///     Returns a copy of the layout without the specified part.
    /// </summary>
    /// <param name="part">One of the enumeration values that specifies the part to remove.</param>
    /// <returns>A new layout without <paramref name="part" />.</returns>
    public FieldLayout Without(FieldPart part)
    {
        return new FieldLayout(
            Orientation,
            BeforeControl.Where(p => p != part),
            AfterControl.Where(p => p != part)
        );
    }
}
