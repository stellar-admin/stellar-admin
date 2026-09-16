using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using StellarAdmin.Dashboard.Resources.Builders;
using StellarAdmin.Dashboard.Resources.Infrastructure.Expressions;

namespace StellarAdmin.Dashboard.EntityFrameworkCore;

/// <summary>
///     Configures the screens for an EF Core entity.
/// </summary>
public sealed class EfCoreResourceBuilder<TContext, TEntity> : ResourceBuilder<TEntity>
    where TContext : DbContext
    where TEntity : class
{
    private readonly EfCoreResourceOptions<TContext, TEntity> _options;

    /// <summary>
    ///     The authorization policy required to access the resource.
    /// </summary>
    public string? AuthorizationPolicy
    {
        get => _options.AuthorizationPolicy;
        set => _options.AuthorizationPolicy = value;
    }

    internal EfCoreResourceBuilder(EfCoreResourceOptions<TContext, TEntity> options)
        : base(options)
    {
        _options = options;
    }

    /// <summary>Adds a reference using an EF foreign key, navigation, and display property.</summary>
    public EfCoreResourceBuilder<TContext, TEntity> AddReference<TKey, TTarget>(
        Expression<Func<TEntity, TKey>> key,
        Expression<Func<TEntity, TTarget?>> navigation,
        Expression<Func<TTarget, string>> display,
        Action<EfCoreReferenceBuilder<TTarget>>? configure = null
    )
        where TTarget : class
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(navigation);
        ArgumentNullException.ThrowIfNull(display);

        var fieldName = FieldExpressionHelper.ExtractDirectPropertyName(key);
        if (
            fieldName is null
            || FieldExpressionHelper.ExtractDirectPropertyName(navigation) is null
            || FieldExpressionHelper.ExtractDirectPropertyName(display) is null
        )
        {
            throw new ArgumentException("Reference selectors must select direct properties.");
        }

        if (_options.References.Any(reference => reference.FieldName == fieldName))
        {
            throw new InvalidOperationException($"Reference '{fieldName}' is already registered.");
        }

        var builder = new EfCoreReferenceBuilder<TTarget>();
        configure?.Invoke(builder);
        _options.References.Add(
            new EfCoreReference<TEntity, TKey, TTarget>(
                fieldName,
                key,
                navigation,
                display,
                builder.ChoiceOptions
            )
        );

        return this;
    }
}
