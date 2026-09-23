using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StellarAdmin.Dashboard.Resources.Options;

namespace StellarAdmin.Dashboard.EntityFrameworkCore;

/// <summary>
///     Registers EF Core resources.
/// </summary>
public static class StellarAdminDashboardBuilderExtensions
{
    /// <summary>
    ///     Adds an EF Core resource.
    /// </summary>
    public static EfCoreResourceBuilder<TContext, TEntity> AddEfCoreResource<TContext, TEntity>(
        this StellarAdminDashboardBuilder builder
    )
        where TContext : DbContext
        where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(builder);

        var resource = builder.AddResource<TEntity>();
        resource.UseDataSource<EfCoreResourceDataSource<TContext, TEntity>>();
        builder
            .Services.AddOptions<ResourceOptions<TEntity>>()
            .Configure(options => options.Index = new EfCoreResourceIndexOptions<TEntity>())
            .PostConfigure<IServiceScopeFactory>(
                (options, scopeFactory) =>
                {
                    // Options are cached. Use a separate scope to read metadata without retaining a DbContext.
                    using var scope = scopeFactory.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<TContext>();
                    var entity =
                        db.Model.FindEntityType(typeof(TEntity))
                        ?? throw new InvalidOperationException(
                            $"{typeof(TEntity).Name} is not mapped in {typeof(TContext).Name}."
                        );
                    var keys = entity.FindPrimaryKey()?.Properties;
                    if (
                        keys is not { Count: 1 }
                        || keys[0].PropertyInfo is not { GetMethod.IsPublic: true } property
                        || !(
                            property.PropertyType == typeof(int)
                            || property.PropertyType == typeof(long)
                            || property.PropertyType == typeof(Guid)
                            || property.PropertyType == typeof(string)
                        )
                    )
                    {
                        throw new InvalidOperationException(
                            "EF resources require one public CLR primary-key property of type int, long, Guid, or string."
                        );
                    }

                    options.KeyPropertyName = property.Name;
                    options.KeySelector = item =>
                        Convert.ToString(property.GetValue(item), CultureInfo.InvariantCulture)!;

                    var references = scope
                        .ServiceProvider.GetRequiredService<
                            IOptions<EfCoreResourceReferences<TEntity>>
                        >()
                        .Value.Items;
                    foreach (var reference in references)
                    {
                        reference.Validate(entity);
                        foreach (
                            var column in options.Index.Columns.Where(column =>
                                column.FieldName == reference.FieldName
                            )
                        )
                        {
                            column.DisplayExpression = reference.DisplayExpression;
                            column.Title ??= reference.NavigationName;
                        }

                        if (options.Create?.ModelType == typeof(TEntity))
                        {
                            SetReferenceFieldTitle(options.Create, reference);
                        }
                        if (options.Edit?.ModelType == typeof(TEntity))
                        {
                            SetReferenceFieldTitle(options.Edit, reference);
                        }
                    }

                    if (options.Create is { } create && create.ModelType == typeof(TEntity))
                    {
                        ValidateEntityFormFields<TEntity>(entity, create);
                    }
                    if (options.Edit is { } edit && edit.ModelType == typeof(TEntity))
                    {
                        ValidateEntityFormFields<TEntity>(entity, edit);
                    }
                }
            );

        return new(builder.Services);
    }

    /// <summary>
    ///     Adds and configures an EF Core resource.
    /// </summary>
    public static StellarAdminDashboardBuilder AddEfCoreResource<TContext, TEntity>(
        this StellarAdminDashboardBuilder builder,
        Action<EfCoreResourceBuilder<TContext, TEntity>> configure
    )
        where TContext : DbContext
        where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(configure);
        configure(builder.AddEfCoreResource<TContext, TEntity>());

        return builder;
    }

    private static void SetReferenceFieldTitle<TEntity>(
        ResourceFormOptions form,
        EfCoreReference<TEntity> reference
    )
        where TEntity : class
    {
        foreach (var field in form.Fields.Where(field => field.FieldName == reference.FieldName))
        {
            field.Title ??= reference.NavigationName;
        }
    }

    private static void ValidateEntityFormFields<TEntity>(
        IEntityType entity,
        ResourceFormOptions form
    )
    {
        foreach (var field in form.Fields)
        {
            var property = EfCoreFormFieldMetadata.FindProperty(entity, field.FieldName);
            if (
                property?.PropertyInfo is not { SetMethod.IsPublic: true }
                || property.IsPrimaryKey()
                || property.IsConcurrencyToken
                || property.ValueGenerated != ValueGenerated.Never
            )
            {
                throw new InvalidOperationException(
                    $"{typeof(TEntity).Name}.{field.FieldName} cannot be used as an EF resource form field."
                );
            }
        }
    }
}
