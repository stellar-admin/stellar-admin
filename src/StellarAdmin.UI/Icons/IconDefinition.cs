namespace StellarAdmin.UI.Icons;

/// <summary>
///     An icon: the attributes of its <c>&lt;svg&gt;</c> element and the shapes drawn inside it.
/// </summary>
public sealed class IconDefinition
{
    /// <summary>
    ///     Creates an icon from a copy of <paramref name="attributes" /> and
    ///     <paramref name="shapes" />, so later changes to them don't affect it.
    /// </summary>
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
    ///     The attributes rendered on the <c>&lt;svg&gt;</c> element, such as <c>viewBox</c> and
    ///     <c>stroke</c>. Attributes on <c>&lt;sa-icon&gt;</c> take precedence.
    /// </summary>
    public IReadOnlyDictionary<string, string> Attributes { get; }

    /// <summary>
    ///     The shapes rendered inside the <c>&lt;svg&gt;</c>, in order.
    /// </summary>
    public IReadOnlyList<SvgShape> Shapes { get; }
}
