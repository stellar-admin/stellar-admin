using System.Linq.Expressions;
using StellarAdmin.Dashboard.Resources.Options;

namespace StellarAdmin.Dashboard.Resources.Builders;

/// <summary>
///     Configures the fields displayed on a resource's form.
/// </summary>
public class ResourceFieldsBuilder<TResource>
{
    private readonly Action<Action<IFormScope>> _configure;

    internal ResourceFieldsBuilder(Action<Action<IFormScope>> configure) => _configure = configure;

    /// <summary>
    ///     Adds a field.
    /// </summary>
    public ResourceFieldBuilder Add<TProperty>(Expression<Func<TResource, TProperty>> field)
    {
        ArgumentNullException.ThrowIfNull(field);

        var properties = ResourcePropertyPath.GetProperties(field);
        if (
            properties is null
            || properties.Any(property => property.GetMethod?.IsPublic != true)
            || properties[^1].SetMethod?.IsPublic != true
        )
        {
            throw new ArgumentException(
                "Select a property path with public getters and a public setter on its final property.",
                nameof(field)
            );
        }

        var fieldBuilder = new ResourceFieldBuilder(
            field,
            ResourcePropertyPath.GetName(properties),
            properties
        );
        _configure(scope => scope.Items.Add(fieldBuilder.Build()));

        return fieldBuilder;
    }

    /// <summary>
    ///     Adds and configures a field.
    /// </summary>
    public ResourceFieldsBuilder<TResource> Add<TProperty>(
        Expression<Func<TResource, TProperty>> field,
        Action<ResourceFieldBuilder> configure
    )
    {
        ArgumentNullException.ThrowIfNull(configure);

        configure(Add(field));

        return this;
    }

    /// <summary>
    ///     Adds a group of related fields.
    /// </summary>
    public ResourceGroupBuilder<TResource> AddGroup() =>
        AddContainer(
            configure => new ResourceGroupBuilder<TResource>(configure),
            group => group.Build()
        );

    /// <summary>
    ///     Adds and configures a group of related fields.
    /// </summary>
    public ResourceFieldsBuilder<TResource> AddGroup(
        Action<ResourceGroupBuilder<TResource>> configure
    )
    {
        ArgumentNullException.ThrowIfNull(configure);

        configure(AddGroup());

        return this;
    }

    /// <summary>
    ///     Adds a titled section.
    /// </summary>
    public ResourceSectionBuilder<TResource> AddSection(string title)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);

        return AddContainer(
            configure => new ResourceSectionBuilder<TResource>(title, configure),
            section => section.Build()
        );
    }

    /// <summary>
    ///     Adds and configures a titled section.
    /// </summary>
    public ResourceFieldsBuilder<TResource> AddSection(
        string title,
        Action<ResourceSectionBuilder<TResource>> configure
    )
    {
        ArgumentNullException.ThrowIfNull(configure);

        configure(AddSection(title));

        return this;
    }

    /// <summary>
    ///     Removes all fields and containers in this scope.
    /// </summary>
    public ResourceFieldsBuilder<TResource> Clear()
    {
        _configure(scope => scope.Items.Clear());

        return this;
    }

    /// <summary>
    ///     Sets the number of columns, starting at the medium breakpoint.
    /// </summary>
    /// <remarks>
    ///     Defaults to 1. Below the medium breakpoint, the fields use one column.
    /// </remarks>
    public ResourceFieldsBuilder<TResource> Columns(int count)
    {
        var columns = FormGridColumnDefinitions.FromCount(count);
        _configure(scope => scope.Columns = columns);

        return this;
    }

    /// <summary>
    ///     Sets the number of columns at each breakpoint.
    /// </summary>
    public ResourceFieldsBuilder<TResource> Columns(Action<GridColumnsBuilder> configure)
    {
        var columns = GridColumnsBuilder.Build(configure);
        _configure(scope => scope.Columns = columns);

        return this;
    }

    private TBuilder AddContainer<TBuilder>(
        Func<Action<Action<IFormScope>>, TBuilder> createBuilder,
        Func<TBuilder, FormContainerOptions> build
    )
    {
        var configuration = new List<Action<IFormScope>>();
        var builder = createBuilder(configuration.Add);
        _configure(scope =>
        {
            var container = build(builder);
            foreach (var configure in configuration)
            {
                configure(container);
            }

            scope.Items.Add(container);
        });

        return builder;
    }
}
