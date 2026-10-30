using StellarAdmin.Widgets;

namespace StellarAdmin.IntegrationTests.Fixtures;

public sealed record SalesParameters(string Region, [property: WidgetInput] int Days);

public sealed record SalesSummary(string Region, int Days, decimal Total);

public sealed class SalesHandler : IWidgetHandler<SalesParameters, SalesSummary>
{
    public Task<SalesSummary?> LoadAsync(
        SalesParameters parameters,
        WidgetLoadContext context,
        CancellationToken cancellationToken
    ) =>
        Task.FromResult<SalesSummary?>(
            new(parameters.Region, parameters.Days, parameters.Days * 100m)
        );
}

public sealed record Greeting(string Text);

public sealed class GreetingHandler : IWidgetHandler<Greeting>
{
    public Task<Greeting?> LoadAsync(
        WidgetLoadContext context,
        CancellationToken cancellationToken
    ) =>
        Task.FromResult<Greeting?>(
            new($"Hello, {context.User.Identity?.Name ?? "guest"} from {context.Name}")
        );
}

public sealed class NothingToShowHandler : IWidgetHandler<Greeting>
{
    public Task<Greeting?> LoadAsync(
        WidgetLoadContext context,
        CancellationToken cancellationToken
    ) => Task.FromResult<Greeting?>(null);
}

public sealed class FailingWidgetHandler : IWidgetHandler<Greeting>
{
    public Task<Greeting?> LoadAsync(
        WidgetLoadContext context,
        CancellationToken cancellationToken
    ) => throw new InvalidOperationException("The sales service is down");
}

// Implements both handler interfaces, so the widget's model isn't known
public sealed class AmbiguousWidgetHandler
    : IWidgetHandler<Greeting>,
        IWidgetHandler<SalesParameters, SalesSummary>
{
    public Task<Greeting?> LoadAsync(
        WidgetLoadContext context,
        CancellationToken cancellationToken
    ) => Task.FromResult<Greeting?>(null);

    public Task<SalesSummary?> LoadAsync(
        SalesParameters parameters,
        WidgetLoadContext context,
        CancellationToken cancellationToken
    ) => Task.FromResult<SalesSummary?>(null);
}
