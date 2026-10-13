using System.Linq.Expressions;
using StellarAdmin.Dashboard.Resources.Options;

namespace StellarAdmin.Dashboard.Resources.Builders;

/// <summary>
///     Configures the fields displayed on a resource's view page.
/// </summary>
public class ResourceViewFieldsBuilder<TModel>
{
    private readonly Action<Action<IFormScope>> _configure;

    internal ResourceViewFieldsBuilder(Action<Action<IFormScope>> configure) =>
        _configure = configure;

    /// <summary>
    ///     Adds a field.
    /// </summary>
    public ResourceDisplayFieldBuilder Add<TProperty>(Expression<Func<TModel, TProperty>> field) =>
        FormScopeBuilder.AddDisplayField(_configure, field, nameof(field));

    /// <summary>
    ///     Adds and configures a field.
    /// </summary>
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
    ///     Adds a group of related fields.
    /// </summary>
    public ResourceViewGroupBuilder<TModel> AddGroup() =>
        FormScopeBuilder.AddContainer(
            _configure,
            configure => new ResourceViewGroupBuilder<TModel>(configure),
            group => group.Build()
        );

    /// <summary>
    ///     Adds and configures a group of related fields.
    /// </summary>
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
    ///     Removes all fields and containers in this scope.
    /// </summary>
    public ResourceViewFieldsBuilder<TModel> Clear()
    {
        _configure(scope => scope.Items.Clear());

        return this;
    }

    /// <summary>
    ///     Sets the number of columns, starting at the medium breakpoint.
    /// </summary>
    /// <remarks>
    ///     Defaults to 1. Breakpoints are measured against the page's width. Below the medium breakpoint, the
    ///     fields use one column.
    /// </remarks>
    public ResourceViewFieldsBuilder<TModel> Columns(int count)
    {
        var columns = FormGridColumnDefinitions.FromCount(count);
        _configure(scope => scope.Columns = columns);

        return this;
    }

    /// <summary>
    ///     Sets the number of columns at each breakpoint.
    /// </summary>
    /// <remarks>
    ///     Breakpoints are measured against the page's width.
    /// </remarks>
    public ResourceViewFieldsBuilder<TModel> Columns(Action<GridColumnsBuilder> configure)
    {
        var columns = GridColumnsBuilder.Build(configure);
        _configure(scope => scope.Columns = columns);

        return this;
    }
}
