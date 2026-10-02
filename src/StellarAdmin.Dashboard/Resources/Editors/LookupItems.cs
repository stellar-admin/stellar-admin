using System.Globalization;
using Microsoft.Extensions.DependencyInjection;

namespace StellarAdmin.Dashboard.Resources.Editors;

internal abstract class LookupItems
{
    public abstract Task<LookupItem?> FindAsync(
        IServiceProvider services,
        FieldEditorContext context,
        CancellationToken cancellationToken
    );
}

internal sealed class LookupItems<TSource, TEntity, TValue>(
    Func<TEntity, TValue> value,
    Func<TEntity, string> text,
    Func<TEntity, string?>? description
) : LookupItems
    where TSource : class, ILookupSource<TEntity, TValue>
{
    // Search results submit the selector's value
    public Func<TEntity, TValue> Value { get; } = value;

    public override async Task<LookupItem?> FindAsync(
        IServiceProvider services,
        FieldEditorContext context,
        CancellationToken cancellationToken
    )
    {
        switch (context.Value)
        {
            case null:
                return null;
            case TValue current:
                var entity = await services
                    .GetRequiredService<TSource>()
                    .FindAsync(current, cancellationToken);

                // A value the source no longer has is still displayed, so the selection stays visible
                return entity is null
                    ? new LookupItem(Format(current), null)
                    : new LookupItem(text(entity), description?.Invoke(entity));
            default:
                throw new InvalidOperationException(
                    $"LookupEditor on {context.FieldName} has a {context.Value.GetType().Name} value, but its items use {typeof(TValue).Name}."
                );
        }
    }

    private static string Format(TValue current) =>
        Convert.ToString(current, CultureInfo.InvariantCulture) ?? "";
}

internal sealed record LookupItem(string Text, string? Description);
