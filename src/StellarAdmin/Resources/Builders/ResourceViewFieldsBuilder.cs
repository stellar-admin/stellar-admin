using System.Linq.Expressions;
using StellarAdmin.Resources.Options;

namespace StellarAdmin.Resources.Builders;

/// <summary>
///     Configures the fields displayed on a resource's view page.
/// </summary>
/// <typeparam name="TModel">The type of the model the page displays.</typeparam>
public abstract class ResourceViewFieldsBuilder<TModel>
{
    private readonly Action<Action<IFormScope>> _configure;

    internal ResourceViewFieldsBuilder(Action<Action<IFormScope>> configure) =>
        _configure = configure;

    /// <summary>
    ///     Adds a field.
    /// </summary>
    /// <typeparam name="TProperty">The type of the selected property.</typeparam>
    /// <param name="field">
    ///     An expression that selects the property the field shows, or computes the value it shows,
    ///     such as <c>tour =&gt; tour.Nights * tour.NightlyRate</c>.
    /// </param>
    /// <returns>A builder that configures the field.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="field" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     <paramref name="field" /> selects a property path without public getters.
    /// </exception>
    public ResourceDisplayFieldBuilder Add<TProperty>(Expression<Func<TModel, TProperty>> field) =>
        FormScopeBuilder.AddDisplayField(_configure, field, nameof(field));

    /// <summary>
    ///     Adds and configures a field.
    /// </summary>
    /// <typeparam name="TProperty">The type of the selected property.</typeparam>
    /// <param name="field">
    ///     An expression that selects the property the field shows, or computes the value it shows,
    ///     such as <c>tour =&gt; tour.Nights * tour.NightlyRate</c>.
    /// </param>
    /// <param name="configure">A delegate that configures the field.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="field" /> or <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     <paramref name="field" /> selects a property path without public getters.
    /// </exception>
    public ResourceViewFieldsBuilder<TModel> Add<TProperty>(
        Expression<Func<TModel, TProperty>> field,
        Action<ResourceDisplayFieldBuilder> configure
    )
    {
        ArgumentNullException.ThrowIfNull(configure);

        configure(Add(field));

        return this;
    }

    /// <summary>
    ///     Adds an untitled group of related fields.
    /// </summary>
    /// <returns>A builder that adds and configures the group's fields.</returns>
    public ResourceViewGroupBuilder<TModel> AddGroup() =>
        FormScopeBuilder.AddContainer(
            _configure,
            configure => new ResourceViewGroupBuilder<TModel>(configure),
            group => group.Build()
        );

    /// <summary>
    ///     Adds and configures an untitled group of related fields.
    /// </summary>
    /// <param name="configure">A delegate that adds and configures the group's fields.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
    public ResourceViewFieldsBuilder<TModel> AddGroup(
        Action<ResourceViewGroupBuilder<TModel>> configure
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
    /// <returns>A builder that adds and configures the section's fields.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="title" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     <paramref name="title" /> is empty or whitespace.
    /// </exception>
    public ResourceViewSectionBuilder<TModel> AddSection(string title)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);

        return FormScopeBuilder.AddContainer(
            _configure,
            configure => new ResourceViewSectionBuilder<TModel>(title, configure),
            section => section.Build()
        );
    }

    /// <summary>
    ///     Adds and configures a titled section.
    /// </summary>
    /// <param name="title">The section title.</param>
    /// <param name="configure">A delegate that adds and configures the section's fields.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="title" /> or <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     <paramref name="title" /> is empty or whitespace.
    /// </exception>
    public ResourceViewFieldsBuilder<TModel> AddSection(
        string title,
        Action<ResourceViewSectionBuilder<TModel>> configure
    )
    {
        ArgumentNullException.ThrowIfNull(configure);

        configure(AddSection(title));

        return this;
    }

    /// <summary>
    ///     Removes the fields, sections and groups added so far.
    /// </summary>
    /// <returns>The builder, so that calls can be chained.</returns>
    public ResourceViewFieldsBuilder<TModel> Clear()
    {
        _configure(scope => scope.Items.Clear());

        return this;
    }

    /// <summary>
    ///     Sets the number of columns the fields use when there is room for them.
    /// </summary>
    /// <param name="count">The number of columns, from 1 to 12.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <remarks>
    ///     The columns apply when the fields' container is at least 40rem wide. A narrower
    ///     container shows one column. Without columns, the fields use one column.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    ///     <paramref name="count" /> is less than 1 or greater than 12.
    /// </exception>
    public ResourceViewFieldsBuilder<TModel> Columns(int count)
    {
        var columns = FormGridColumnDefinitions.FromCount(count);
        _configure(scope => scope.Columns = columns);

        return this;
    }

    /// <summary>
    ///     Sets the number of columns the fields use at each breakpoint.
    /// </summary>
    /// <param name="configure">
    ///     A delegate that sets the number of columns at each breakpoint.
    /// </param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <remarks>
    ///     Breakpoints are measured against the width of the fields' container.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
    public ResourceViewFieldsBuilder<TModel> Columns(Action<GridColumnsBuilder> configure)
    {
        var columns = GridColumnsBuilder.Build(configure);
        _configure(scope => scope.Columns = columns);

        return this;
    }
}
