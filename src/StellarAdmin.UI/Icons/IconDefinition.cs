namespace StellarAdmin.UI.Icons;

/// <summary>
///     An icon: the attributes of its <c>&lt;svg&gt;</c> element and the shapes drawn inside it.
/// </summary>
public sealed class IconDefinition
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="IconDefinition" /> class.
    /// </summary>
    /// <remarks>
    ///     The icon keeps copies of <paramref name="attributes" /> and <paramref name="shapes" />,
    ///     so later changes to them don't affect it.
    /// </remarks>
    /// <param name="attributes">The attributes to render on the <c>&lt;svg&gt;</c> element.</param>
    /// <param name="shapes">The shapes to render inside the <c>&lt;svg&gt;</c> element, in order.</param>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="attributes" /> or <paramref name="shapes" /> is <see langword="null" />.
    /// </exception>
    public IconDefinition(
        IReadOnlyDictionary<string, string> attributes,
        IReadOnlyList<SvgShape> shapes
    )
    {
        ArgumentNullException.ThrowIfNull(attributes);
        ArgumentNullException.ThrowIfNull(shapes);

        Attributes = new Dictionary<string, string>(attributes).AsReadOnly();
        Shapes = shapes.ToArray().AsReadOnly();
    }

    /// <summary>
    ///     Gets the attributes rendered on the <c>&lt;svg&gt;</c> element.
    /// </summary>
    /// <value>
    ///     The attribute values, keyed by attribute name, such as <c>viewBox</c> and <c>stroke</c>.
    ///     Attributes on <c>&lt;sa-icon&gt;</c> take precedence.
    /// </value>
    public IReadOnlyDictionary<string, string> Attributes { get; }

    /// <summary>
    ///     Gets the shapes rendered inside the <c>&lt;svg&gt;</c> element.
    /// </summary>
    /// <value>
    ///     The shapes, in the order they're drawn.
    /// </value>
    public IReadOnlyList<SvgShape> Shapes { get; }
}
