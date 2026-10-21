using System.Linq.Expressions;
using StellarAdmin.Diagnostics;
using StellarAdmin.Resources.Options;

namespace StellarAdmin.Resources.Builders;

/// <summary>
///     Configures saving a new resource and starting the next one in the same step.
/// </summary>
/// <typeparam name="TModel">The type of the model the create form binds to.</typeparam>
public sealed class ResourceAddAnotherBuilder<TModel>
{
    private readonly Action<Action<ResourceAddAnotherOptions>> _configure;

    /// <summary>
    ///     Sets the label of the button that saves and starts the next resource.
    /// </summary>
    /// <value>
    ///     The default is the label set with
    ///     <see cref="ResourceCreateLabelsBuilder.AddAnotherLabel" />.
    /// </value>
    public string? Label
    {
        set => _configure(options => options.Label = value);
    }

    internal ResourceAddAnotherBuilder(Action<Action<ResourceAddAnotherOptions>> configure) =>
        _configure = configure;

    /// <summary>
    ///     Carries a field's saved value into the next new resource.
    /// </summary>
    /// <typeparam name="TProperty">The type of the field's property.</typeparam>
    /// <param name="property">An expression that selects the property of the field.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="property" /> is <see langword="null" />.
    /// </exception>
    /// <remarks>
    ///     The property must be an editable field of the create form. Otherwise, or when the
    ///     expression doesn't select a property path, it is a configuration problem, and the call is
    ///     ignored.
    /// </remarks>
    public ResourceAddAnotherBuilder<TModel> KeepForNext<TProperty>(
        Expression<Func<TModel, TProperty>> property
    )
    {
        ArgumentNullException.ThrowIfNull(property);

        // KeepForNext's check reports a name that matches no editable field, as this one never does
        var fieldName = ResourcePropertyPath.GetProperties(property) is { } properties
            ? ResourcePropertyPath.GetName(properties)
            : property.ToString();
        var source = ConfigurationSource.Capture();
        _configure(options =>
        {
            if (!options.KeptFieldNames.Contains(fieldName))
            {
                options.KeptFieldNames.Add(fieldName);
                options.KeptFieldSources[fieldName] = source;
            }
        });

        return this;
    }
}
