using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Resources;

namespace StellarAdmin.EntityFrameworkCore;

// Binds a many-to-many navigation as the keys of its related entities. The related entities a form adds are
// loaded through the request's context, so they're tracked when the data source saves the entity.
internal sealed class ManyToManyFieldBinder<TRelated, TKey>(
    Type contextType,
    PropertyInfo navigation,
    PropertyInfo key
) : FieldBinder, IManyToManyFieldBinder
    where TRelated : class
    where TKey : notnull
{
    private readonly Func<TRelated, TKey> _key = related => (TKey)key.GetValue(related)!;

    public string NavigationName => navigation.Name;

    public override async Task BindAsync(
        FieldBindingContext context,
        CancellationToken cancellationToken
    )
    {
        var keys = new List<TKey>();
        var converter = TypeDescriptor.GetConverter(typeof(TKey));
        foreach (var value in context.Values)
        {
            if (string.IsNullOrEmpty(value))
            {
                continue;
            }

            // Values are posted as the editors format them, in the current culture
            try
            {
                keys.Add(
                    (TKey)converter.ConvertFromString(null, CultureInfo.CurrentCulture, value)!
                );
            }
            catch (Exception exception)
                when (exception is ArgumentException or FormatException or NotSupportedException)
            {
                context.AddError($"'{value}' is not a valid choice.");
            }
        }

        var collection = GetCollection(context.Model);
        var current = collection.Select(_key).ToHashSet();
        var added = keys.Distinct().Where(value => !current.Contains(value)).ToList();
        var loaded =
            added.Count == 0
                ? []
                : await ((DbContext)context.Services.GetRequiredService(contextType))
                    .Set<TRelated>()
                    .Where(related => added.Contains(EF.Property<TKey>(related, key.Name)))
                    .ToListAsync(cancellationToken);
        if (loaded.Count < added.Count)
        {
            var found = loaded.Select(_key).ToHashSet();
            foreach (var missing in added.Where(value => !found.Contains(value)))
            {
                context.AddError($"'{missing}' is not a valid choice.");
            }
        }

        var selected = keys.ToHashSet();
        foreach (
            var related in collection.Where(related => !selected.Contains(_key(related))).ToList()
        )
        {
            collection.Remove(related);
        }

        foreach (var related in loaded)
        {
            collection.Add(related);
        }
    }

    // The stored entity loads its related entities and changes to the model's, through the context's tracked
    // instances, so EF Core only adds and removes the join rows
    public async Task CopyAsync(
        DbContext db,
        object target,
        object source,
        CancellationToken cancellationToken
    )
    {
        var entry = db.Entry(target).Collection(navigation.Name);
        if (!entry.IsLoaded)
        {
            await entry.LoadAsync(cancellationToken);
        }

        var collection = GetCollection(target);
        var selected = GetCollection(source).Select(_key).ToHashSet();
        foreach (
            var related in collection.Where(related => !selected.Contains(_key(related))).ToList()
        )
        {
            collection.Remove(related);
        }

        var current = collection.Select(_key).ToHashSet();
        foreach (var value in selected.Where(value => !current.Contains(value)))
        {
            collection.Add(
                await db.Set<TRelated>().FindAsync([value], cancellationToken)
                    ?? throw new InvalidOperationException(
                        $"{typeof(TRelated).Name} '{value}' was not found."
                    )
            );
        }
    }

    public override object GetValue(FieldBindingContext context) =>
        (navigation.GetValue(context.Model) as IEnumerable<TRelated> ?? []).Select(_key).ToList();

    private ICollection<TRelated> GetCollection(object model)
    {
        if (navigation.GetValue(model) is ICollection<TRelated> collection)
        {
            return collection;
        }

        if (navigation.SetMethod?.IsPublic != true)
        {
            throw new InvalidOperationException(
                $"{model.GetType().Name}.{navigation.Name} is null and has no public setter. Initialize the collection."
            );
        }

        collection = new List<TRelated>();
        navigation.SetValue(model, collection);

        return collection;
    }
}
