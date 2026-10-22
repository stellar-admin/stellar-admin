using StellarAdmin.Resources.Options;

namespace StellarAdmin.Resources.Builders;

/// <summary>
///     Configures a field's dependency on another field.
/// </summary>
public sealed class FieldDependencyBuilder
{
    private readonly FieldDependency _dependency;

    internal FieldDependencyBuilder(FieldDependency dependency) => _dependency = dependency;

    /// <summary>
    ///     Sets the text the field shows while the other field has no value.
    /// </summary>
    /// <value>
    ///     The default asks for the other field first, such as <c>Select Country first</c>.
    /// </value>
    /// <remarks>
    ///     The field is disabled while the other field has no value. A select shows the text as its
    ///     empty choice, a lookup on its button, and a choice group in place of its choices.
    /// </remarks>
    public string? EmptyText
    {
        set => _dependency.EmptyText = value;
    }
}
