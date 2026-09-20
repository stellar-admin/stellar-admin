using System.Linq.Expressions;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Dashboard.Resources.Options;

namespace StellarAdmin.Dashboard.Resources.Builders;

/// <summary>
///     Configures the fields displayed on a resource's form.
/// </summary>
public sealed class ResourceFieldsBuilder<TResource>
{
    private readonly IServiceCollection _services;

    internal ResourceFieldsBuilder(IServiceCollection services) => _services = services;

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
        _services.Configure<ResourceOptions<TResource>>(options =>
            options.Create.Fields.Add(fieldBuilder.Build())
        );

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
    ///     Removes all configured fields.
    /// </summary>
    public ResourceFieldsBuilder<TResource> Clear()
    {
        _services.Configure<ResourceOptions<TResource>>(options => options.Create.Fields.Clear());

        return this;
    }
}
