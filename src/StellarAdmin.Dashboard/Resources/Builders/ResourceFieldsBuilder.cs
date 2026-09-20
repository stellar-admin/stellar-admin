using System.Linq.Expressions;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Dashboard.Resources.Options;

namespace StellarAdmin.Dashboard.Resources.Builders;

/// <summary>
///     Configures the fields displayed on a resource's form.
/// </summary>
public class ResourceFieldsBuilder<TResource>
{
    private readonly Action<Action<IList<FormItemOptions>>> _configure;

    internal ResourceFieldsBuilder(IServiceCollection services)
        : this(configure =>
            services.Configure<ResourceOptions<TResource>>(options =>
                configure(options.Create.Items)
            )
        ) { }

    internal ResourceFieldsBuilder(Action<Action<IList<FormItemOptions>>> configure) =>
        _configure = configure;

    /// <summary>
    ///     Adds a field.
    /// </summary>
    public ResourceFieldBuilder Add<TProperty>(Expression<Func<TResource, TProperty>> field)
    {
        ArgumentNullException.ThrowIfNull(field);

        if (
            field.Body is not MemberExpression { Member: PropertyInfo property } member
            || member.Expression != field.Parameters[0]
            || property.GetMethod?.IsPublic != true
            || property.SetMethod?.IsPublic != true
        )
        {
            throw new ArgumentException(
                "Select a direct property with a public getter and setter.",
                nameof(field)
            );
        }

        var fieldBuilder = new ResourceFieldBuilder(field);
        _configure(items => items.Add(fieldBuilder.Build()));

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
    public ResourceFieldsBuilder<TResource> AddGroup() =>
        AddContainer(() => new FormGroupOptions());

    /// <summary>
    ///     Adds and configures a group of related fields.
    /// </summary>
    public ResourceFieldsBuilder<TResource> AddGroup(
        Action<ResourceFieldsBuilder<TResource>> configure
    )
    {
        ArgumentNullException.ThrowIfNull(configure);

        configure(AddGroup());

        return this;
    }

    /// <summary>
    ///     Adds a row whose fields and groups form equally sized columns.
    /// </summary>
    public ResourceFieldsBuilder<TResource> AddRow() => AddContainer(() => new FormRowOptions());

    /// <summary>
    ///     Adds and configures a row.
    /// </summary>
    public ResourceFieldsBuilder<TResource> AddRow(
        Action<ResourceFieldsBuilder<TResource>> configure
    )
    {
        ArgumentNullException.ThrowIfNull(configure);

        configure(AddRow());

        return this;
    }

    /// <summary>
    ///     Adds a titled section.
    /// </summary>
    public ResourceSectionBuilder<TResource> AddSection(string title)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);

        var configuration = new List<Action<IList<FormItemOptions>>>();
        var section = new ResourceSectionBuilder<TResource>(title, configuration.Add);
        _configure(items =>
        {
            var options = section.Build();
            foreach (var configure in configuration)
            {
                configure(options.MutableItems);
            }

            items.Add(options);
        });

        return section;
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
        _configure(items => items.Clear());

        return this;
    }

    private ResourceFieldsBuilder<TResource> AddContainer(Func<FormContainerOptions> factory)
    {
        var configuration = new List<Action<IList<FormItemOptions>>>();
        _configure(items =>
        {
            var container = factory();
            foreach (var configure in configuration)
            {
                configure(container.MutableItems);
            }

            items.Add(container);
        });

        return new(configuration.Add);
    }
}
