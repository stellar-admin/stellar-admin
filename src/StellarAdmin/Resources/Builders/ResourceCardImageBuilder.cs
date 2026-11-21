using StellarAdmin.Resources.Options;

namespace StellarAdmin.Resources.Builders;

/// <summary>
///     Configures the image on the cards of a cards data layout.
/// </summary>
public sealed class ResourceCardImageBuilder
{
    private readonly Action<Action<ResourceCardImageOptions>> _configure;

    /// <summary>
    ///     Sets the image's aspect ratio.
    /// </summary>
    /// <value>
    ///     The shape. The default is <see cref="ResourceCardImageShape.Widescreen" />.
    /// </value>
    public ResourceCardImageShape Shape
    {
        set => _configure(options => options.Shape = value);
    }

    internal ResourceCardImageBuilder(Action<Action<ResourceCardImageOptions>> configure) =>
        _configure = configure;
}
