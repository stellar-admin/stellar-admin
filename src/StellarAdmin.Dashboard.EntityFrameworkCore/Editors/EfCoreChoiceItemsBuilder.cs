using System.Linq.Expressions;

namespace StellarAdmin.Dashboard.EntityFrameworkCore;

/// <summary>
///     Configures choices loaded from an EF Core entity set.
/// </summary>
public sealed class EfCoreChoiceItemsBuilder<TEntity, TValue>
    where TEntity : class
{
    private readonly EfCoreChoiceItemsOptions<TEntity, TValue> _options;

    internal EfCoreChoiceItemsBuilder(EfCoreChoiceItemsOptions<TEntity, TValue> options)
    {
        _options = options;
    }

    /// <summary>
    ///     Adds an empty choice with the specified text before the entity choices.
    /// </summary>
    public EfCoreChoiceItemsBuilder<TEntity, TValue> IncludeEmptyOption(string text)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(text);

        _options.EmptyOptionText = text;

        return this;
    }

    /// <summary>
    ///     Orders the choices by an entity property. Grouped choices keep this order within their group.
    /// </summary>
    public EfCoreChoiceItemsBuilder<TEntity, TValue> OrderBy<TSort>(
        Expression<Func<TEntity, TSort>> selector
    )
    {
        ArgumentNullException.ThrowIfNull(selector);

        _options.OrderQuery = query => query.OrderBy(selector);

        return this;
    }

    /// <summary>
    ///     Displays secondary text below each choice's text, in editors that display descriptions.
    /// </summary>
    public EfCoreChoiceItemsBuilder<TEntity, TValue> UseDescription(
        Expression<Func<TEntity, string?>> description
    )
    {
        ArgumentNullException.ThrowIfNull(description);

        _options.DescriptionExpression = description;

        return this;
    }

    /// <summary>
    ///     Groups the choices by the selected heading, in editors that display groups. Choices with a null heading
    ///     are not grouped.
    /// </summary>
    public EfCoreChoiceItemsBuilder<TEntity, TValue> UseGroup(
        Expression<Func<TEntity, string?>> group
    )
    {
        ArgumentNullException.ThrowIfNull(group);

        _options.GroupExpression = group;

        return this;
    }
}
