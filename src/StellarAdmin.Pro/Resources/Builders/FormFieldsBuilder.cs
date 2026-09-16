using System.Linq.Expressions;
using StellarAdmin.Pro.Resources.Infrastructure.Expressions;
using StellarAdmin.Pro.Resources.Options;

namespace StellarAdmin.Pro.Resources.Builders;

/// <summary>
///     Configures the fields of a form page.
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
public class FormFieldsBuilder<TEntity>
    where TEntity : class
{
    private readonly List<FormItemOptions> _items;

    internal FormFieldsBuilder(List<FormItemOptions> items)
    {
        _items = items;
    }

    /// <summary>
    ///     Adds a field bound to the property selected by <paramref name="field" />,
    ///     after the fields already configured.
    /// </summary>
    /// <param name="field">
    ///     An expression selecting a property directly on <typeparamref name="TEntity" />,
    ///     like <c>x => x.FirstName</c>. Nested property chains are not supported.
    /// </param>
    /// <exception cref="ArgumentException">
    ///     <paramref name="field" /> does not select a property directly on
    ///     <typeparamref name="TEntity" />.
    /// </exception>
    public FormFieldBuilder Add<TProp>(Expression<Func<TEntity, TProp>> field)
    {
        ArgumentNullException.ThrowIfNull(field);

        var fieldName =
            FieldExpressionHelper.ExtractDirectPropertyName(field)
            ?? throw new ArgumentException(
                $"A form field must select a property directly on {typeof(TEntity).Name}, "
                    + "like x => x.FirstName. Nested property chains, method calls, and "
                    + "indexers cannot bind a posted value and are not supported.",
                nameof(field)
            );

        var formField = new FormFieldOptions(field, fieldName);
        _items.Add(formField);

        return new FormFieldBuilder(formField);
    }

    /// <summary>
    ///     Adds an untitled group of related fields.
    /// </summary>
    public FormContainerBuilder<TEntity> AddGroup(Action<FormContainerBuilder<TEntity>> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        var options = new FormGroupOptions();
        var builder = new FormContainerBuilder<TEntity>(options);
        configure(builder);
        _items.Add(options);

        return builder;
    }

    /// <summary>
    ///     Adds equally sized columns that stack on narrow screens.
    /// </summary>
    public FormContainerBuilder<TEntity> AddRow(Action<FormContainerBuilder<TEntity>> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        var options = new FormRowOptions();
        var builder = new FormContainerBuilder<TEntity>(options);
        configure(builder);
        _items.Add(options);

        return builder;
    }

    /// <summary>
    ///     Adds a titled section of related fields.
    /// </summary>
    public FormSectionBuilder<TEntity> AddSection(
        string title,
        Action<FormSectionBuilder<TEntity>> configure
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentNullException.ThrowIfNull(configure);

        var options = new FormSectionOptions(title);
        var builder = new FormSectionBuilder<TEntity>(options);
        configure(builder);
        _items.Add(options);

        return builder;
    }

    /// <summary>
    ///     Removes all fields and containers in this collection, including defaults.
    /// </summary>
    public FormFieldsBuilder<TEntity> Clear()
    {
        _items.Clear();

        return this;
    }
}
