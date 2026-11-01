using Microsoft.Extensions.DependencyInjection;

namespace StellarAdmin.EntityFrameworkCore;

// A create or edit form's events, and where its model holds the entity
internal sealed class EfCoreFormOptions<TEvents>
    where TEvents : class, new()
{
    public EfCoreFormEntity? Entity { get; set; }

    public TEvents Events { get; set; } = new();

    // The events class UseEvents registers, which replaces Events
    public Type? EventsType { get; set; }

    // Whether events are configured, so a save runs them inside a transaction
    public bool HasEvents { get; set; }

    public TEvents? GetEvents(IServiceProvider services) =>
        !HasEvents ? null
        : EventsType is { } type ? (TEvents)services.GetRequiredService(type)
        : Events;
}
