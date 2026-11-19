using System.Linq.Expressions;

namespace StellarAdmin.Resources.Options;

/// <summary>
///     Stores the configuration of the image on the cards of a cards layout.
/// </summary>
public sealed class ResourceCardImageOptions
{
    /// <summary>
    ///     Gets the expression that selects a record's image URL.
    /// </summary>
    public LambdaExpression Field { get; }

    /// <summary>
    ///     Gets the image's aspect ratio.
    /// </summary>
    /// <value>
    ///     The shape. The default is <see cref="ResourceCardImageShape.Widescreen" />.
    /// </value>
    public ResourceCardImageShape Shape { get; internal set; }

    internal ResourceCardImageOptions(LambdaExpression field) => Field = field;
}
