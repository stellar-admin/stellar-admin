using System.Linq.Expressions;
using StellarAdmin.Resources.Options;

namespace StellarAdmin.Resources.Builders;

/// <summary>
///     Configures the fields of a resource form.
/// </summary>
/// <typeparam name="TResource">The type of the resource the form edits.</typeparam>
public abstract class ResourceFieldsBuilder<TResource>
{
    private readonly Action<Action<IFormScope>> _configure;

    internal ResourceFieldsBuilder(Action<Action<IFormScope>> configure) => _configure = configure;

    /// <summary>
    ///     Adds a field.
    /// </summary>
    /// <typeparam name="TProperty">The type of the field's property.</typeparam>
    /// <param name="field">An expression that selects the property the field binds to.</param>
    /// <returns>A builder that configures the field.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="field" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     <paramref name="field" /> doesn't select a property path with public getters and a
    ///     public setter on its last property.
    /// </exception>
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
    /// <typeparam name="TProperty">The type of the field's property.</typeparam>
    /// <param name="field">An expression that selects the property the field binds to.</param>
    /// <param name="configure">A delegate that configures the field.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="field" /> or <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     <paramref name="field" /> doesn't select a property path with public getters and a
    ///     public setter on its last property.
    /// </exception>
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
    /// <param name="title">The section title.</param>
    /// <returns>A builder that configures the section.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="title" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     <paramref name="title" /> is empty or consists only of white space.
    /// </exception>
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
    /// <param name="title">The section title.</param>
    /// <param name="configure">A delegate that configures the section.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="title" /> or <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     <paramref name="title" /> is empty or consists only of white space.
    /// </exception>
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
    /// <typeparam name="TProperty">The type of the field's property.</typeparam>
    /// <param name="field">An expression that selects the property the field shows.</param>
    /// <returns>A builder that configures the field.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="field" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     <paramref name="field" /> doesn't select a property path with public getters.
    /// </exception>
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
    /// <typeparam name="TProperty">The type of the field's property.</typeparam>
    /// <param name="field">An expression that selects the property the field shows.</param>
    /// <param name="configure">A delegate that configures the field.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="field" /> or <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     <paramref name="field" /> doesn't select a property path with public getters.
    /// </exception>
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
    /// <returns>A builder that configures the group.</returns>
    public ResourceGroupBuilder<TResource> AddGroup() =>
        AddContainer(
            configure => new ResourceGroupBuilder<TResource>(configure),
            group => group.Build()
        );

    /// <summary>
    ///     Adds and configures a group of related fields.
    /// </summary>
    /// <param name="configure">A delegate that configures the group.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
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
    /// <param name="title">The section title.</param>
    /// <returns>A builder that configures the section.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="title" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     <paramref name="title" /> is empty or consists only of white space.
    /// </exception>
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
    /// <param name="title">The section title.</param>
    /// <param name="configure">A delegate that configures the section.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="title" /> or <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     <paramref name="title" /> is empty or consists only of white space.
    /// </exception>
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
    ///     Removes all fields, groups and sections added to this builder so far.
    /// </summary>
    /// <returns>The builder, so that calls can be chained.</returns>
    public ResourceFieldsBuilder<TResource> Clear()
    {
        _configure(scope => scope.Items.Clear());

        return this;
    }

    /// <summary>
    ///     Sets the number of columns when the grid is at least 40rem wide.
    /// </summary>
    /// <param name="count">The number of columns, from 1 to 12.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    ///     <paramref name="count" /> is less than 1 or greater than 12.
    /// </exception>
    /// <remarks>
    ///     The default is 1. A narrower grid uses one column. Breakpoints are measured against
    ///     the width of the form, section or group that holds the fields.
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
    /// <param name="configure">
    ///     A delegate that sets the number of columns at each breakpoint.
    /// </param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
    /// <remarks>
    ///     Breakpoints are measured against the width of the form, section or group that holds
    ///     the fields.
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
