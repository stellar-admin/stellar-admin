using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Diagnostics;
using StellarAdmin.Resources.Editors;

namespace StellarAdmin.Resources;

/// <summary>
///     Gets the items of fields that refer to a resource.
/// </summary>
/// <remarks>
///     Editors get their items from this service when their form renders, because other resources
///     aren't known while a resource is configured.
/// </remarks>
public sealed class ResourceReferences
{
    private readonly ConditionalWeakTable<ReferencedResource, Resolved> _items = new();
    private readonly ConcurrentDictionary<Type, ResourceReference?> _references = new();
    private readonly IServiceProvider _services;

    internal ResourceReferences(IServiceProvider services) => _services = services;

    /// <summary>
    ///     Returns the items of a field that refers to a resource.
    /// </summary>
    /// <param name="resource">The resource, with the field's filters.</param>
    /// <returns>
    ///     The items, or <see langword="null" /> when no resource is registered for the type, or
    ///     the resource can't be referred to.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="resource" /> is <see langword="null" />.
    /// </exception>
    /// <remarks>
    ///     The items are created once for each field. A problem with the resource, or a filter its
    ///     items can't apply, is reported as a configuration problem.
    /// </remarks>
    public ResourceReferenceItems? GetItems(ReferencedResource resource)
    {
        ArgumentNullException.ThrowIfNull(resource);

        return _items.GetValue(resource, Resolve).Items;
    }

    // A record's title, which names it wherever another record refers to it, or null when no resource is registered
    // for its type or the resource can't be referred to
    internal string? FormatTitle(object record)
    {
        ArgumentNullException.ThrowIfNull(record);

        var type = record.GetType();
        if (type.IsValueType || type == typeof(string))
        {
            return null;
        }

        // A proxy, such as one EF Core creates for lazy loading, derives from the resource's type
        var reference = Find(type) ?? (type.BaseType is { } baseType ? Find(baseType) : null);

        return reference is { UnavailableCause: null } ? reference.FormatTitle(record) : null;
    }

    // The label of the fields that refer to the resource, or null when it can't be referred to
    internal string? GetLabel(ReferencedResource resource) =>
        Find(resource.ResourceType) is { UnavailableCause: null } reference
            ? reference.Label
            : null;

    // The reference of the resource registered for a type, found once its options can be built
    private ResourceReference? Find(Type type) =>
        _references.GetOrAdd(
            type,
            resourceType =>
                _services
                    .GetServices<ResourceRegistration>()
                    .FirstOrDefault(registration => registration.ResourceType == resourceType)
                    ?.ResolveReference(_services)
        );

    // The resource is found once its options can be built, and a field's items are created once, with its filters
    private Resolved Resolve(ReferencedResource resource)
    {
        var name = resource.ResourceType.Name.Split('`')[0];
        var reference = Find(resource.ResourceType);
        var cause = reference is null
            ? $"no resource is registered for `{name}`"
            : reference.UnavailableCause;
        if (cause is not null)
        {
            Report(
                resource,
                ConfigurationProblemKind.ReferenceUnavailable,
                $"A field that takes its items from `{name}` has none because {cause}.",
                $"Fix the `{name}` resource, or give the field's editor its items with `UseItems`."
            );

            return new(null);
        }

        if (resource.Filters.Count > 0 && !reference!.AppliesFilters)
        {
            Report(
                resource,
                ConfigurationProblemKind.ReferenceFilterIgnored,
                $"A field that takes its items from `{name}` ignores its `Where` filter because the `{name}` resource loads its records through a data source, which can't apply it.",
                $"Make `{name}` an EF Core resource, or give the field's lookup editor a `LookupSource` that takes the form's values with `UseSource`."
            );

            return new(reference.CreateItems([]));
        }

        return new(reference!.CreateItems(resource.Filters));
    }

    private void Report(
        ReferencedResource resource,
        ConfigurationProblemKind kind,
        string message,
        string fix
    ) =>
        _services
            .GetRequiredService<ConfigurationProblems>()
            .Report(
                new(kind, message, fix) { Source = resource.Source },
                _services
                    .GetRequiredService<IHttpContextAccessor>()
                    .HttpContext?.GetRouteValue("controller") as string
            );

    private sealed record Resolved(ResourceReferenceItems? Items);
}
