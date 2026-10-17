namespace StellarAdmin.UI.Icons;

/// <summary>
///     Represents a shape drawn inside an icon, such as a <c>path</c> or <c>circle</c>.
/// </summary>
public sealed class SvgShape
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="SvgShape" /> class.
    /// </summary>
    /// <remarks>
    ///     The shape keeps a copy of <paramref name="attributes" />, so later changes to it don't
    ///     affect the shape.
    /// </remarks>
    /// <param name="name">The element name of the shape, such as <c>path</c>.</param>
    /// <param name="attributes">The attributes to render on the shape's element.</param>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="name" /> or <paramref name="attributes" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     <paramref name="name" /> is empty.
    /// </exception>
    public SvgShape(string name, IReadOnlyDictionary<string, string> attributes)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        ArgumentNullException.ThrowIfNull(attributes);

        Name = name;
        Attributes = new Dictionary<string, string>(attributes).AsReadOnly();
    }

    /// <summary>
    ///     Gets the element name of the shape, such as <c>path</c>.
    /// </summary>
    public string Name { get; }

    /// <summary>
    ///     Gets the attributes rendered on the shape's element.
    /// </summary>
    /// <value>
    ///     The attribute values, keyed by attribute name, such as <c>d</c>.
    /// </value>
    public IReadOnlyDictionary<string, string> Attributes { get; }
}
