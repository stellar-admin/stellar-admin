using System.Linq.Expressions;
using StellarAdmin.Resources.Options;

namespace StellarAdmin.Resources.Builders;

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
    ///     Adds a titled section that lists read-only fields.
    /// </summary>
    public ResourceDetailsBuilder<TResource> AddDetails(string title)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);

        var details = new ResourceDetailsBuilder<TResource>(title);
        _configure(scope => scope.Items.Add(details.Build()));

        return details;
    }

    /// <summary>
    ///     Adds and configures a titled section that lists read-only fields.
    /// </summary>
    public ResourceFieldsBuilder<TResource> AddDetails(
        string title,
        Action<ResourceDetailsBuilder<TResource>> configure
    )
    {
        ArgumentNullException.ThrowIfNull(configure);

        configure(AddDetails(title));

        return this;
    }

    /// <summary>
    ///     Adds a read-only field.
    /// </summary>
    /// <remarks>
    ///     The field is never bound, so its property needs no setter.
    /// </remarks>
    public ResourceDisplayFieldBuilder AddDisplayField<TProperty>(
        Expression<Func<TResource, TProperty>> field
    )
    {
        return FormScopeBuilder.AddDisplayField(_configure, field, nameof(field));
    }

    /// <summary>
    ///     Adds and configures a read-only field.
    /// </summary>
    /// <remarks>
    ///     The field is never bound, so its property needs no setter.
    /// </remarks>
    public ResourceFieldsBuilder<TResource> AddDisplayField<TProperty>(
        Expression<Func<TResource, TProperty>> field,
        Action<ResourceDisplayFieldBuilder> configure
    )
    {
        ArgumentNullException.ThrowIfNull(configure);

        configure(AddDisplayField(field));

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
    ///     Defaults to 1. Breakpoints are measured against the form's width. Below the medium breakpoint, the
    ///     fields use one column.
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
    /// <remarks>
    ///     Breakpoints are measured against the form's width.
    /// </remarks>
    public ResourceFieldsBuilder<TResource> Columns(Action<GridColumnsBuilder> configure)
    {
        var columns = GridColumnsBuilder.Build(configure);
        _configure(scope => scope.Columns = columns);

        return this;
    }

    private TBuilder AddContainer<TBuilder>(
        Func<Action<Action<IFormScope>>, TBuilder> createBuilder,
        Func<TBuilder, FormContainerOptions> build
    ) => FormScopeBuilder.AddContainer(_configure, createBuilder, build);
}
