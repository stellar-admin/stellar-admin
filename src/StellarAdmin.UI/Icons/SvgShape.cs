namespace StellarAdmin.UI.Icons;

/// <summary>
///     A shape drawn inside an icon, such as a <c>path</c> or <c>circle</c>.
/// </summary>
public sealed class SvgShape
{
    /// <summary>
    ///     Creates a shape from a copy of <paramref name="attributes" />, so later changes to them
    ///     don't affect it.
    /// </summary>
    public SvgShape(string name, IReadOnlyDictionary<string, string> attributes)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        ArgumentNullException.ThrowIfNull(attributes);

        Name = name;
        Attributes = new Dictionary<string, string>(attributes).AsReadOnly();
    }

    /// <summary>
    ///     The element name of the shape, such as <c>path</c>.
    /// </summary>
    public string Name { get; }

    /// <summary>
    ///     The attributes rendered on the shape's element, such as <c>d</c>.
    /// </summary>
    public IReadOnlyDictionary<string, string> Attributes { get; }
}
