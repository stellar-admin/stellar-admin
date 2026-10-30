using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace StellarAdmin.Widgets;

// Runs a widget's handler. A handler implements IWidgetHandler<TModel>, or IWidgetHandler<TParameters, TModel> for
// a widget with parameters, for exactly one model.
internal abstract class WidgetLoader
{
    public abstract Type HandlerType { get; }

    // The handler's parameters type, or null for a handler without parameters
    public abstract Type? ParametersType { get; }

    public static WidgetLoader Create(Type handlerType, IServiceCollection services)
    {
        var interfaces = handlerType
            .GetInterfaces()
            .Where(type =>
                type.IsGenericType
                && (
                    type.GetGenericTypeDefinition() == typeof(IWidgetHandler<>)
                    || type.GetGenericTypeDefinition() == typeof(IWidgetHandler<,>)
                )
            )
            .ToArray();
        if (interfaces.Length != 1)
        {
            return new UnusableWidgetLoader(handlerType, interfaces);
        }

        services.TryAddScoped(handlerType);

        var arguments = interfaces[0].GetGenericArguments();
        var loaderType =
            arguments.Length == 1
                ? typeof(HandlerWidgetLoader<,>).MakeGenericType(arguments[0], handlerType)
                : typeof(HandlerWidgetLoader<,,>).MakeGenericType(
                    arguments[0],
                    arguments[1],
                    handlerType
                );

        return (WidgetLoader)Activator.CreateInstance(loaderType)!;
    }

    public abstract Task<object?> LoadAsync(
        object? parameters,
        WidgetLoadContext context,
        CancellationToken cancellationToken
    );
}

internal sealed class HandlerWidgetLoader<TModel, THandler> : WidgetLoader
    where THandler : class, IWidgetHandler<TModel>
{
    public override Type HandlerType => typeof(THandler);

    public override Type? ParametersType => null;

    public override async Task<object?> LoadAsync(
        object? parameters,
        WidgetLoadContext context,
        CancellationToken cancellationToken
    ) =>
        await context
            .HttpContext.RequestServices.GetRequiredService<THandler>()
            .LoadAsync(context, cancellationToken);
}

internal sealed class HandlerWidgetLoader<TParameters, TModel, THandler> : WidgetLoader
    where THandler : class, IWidgetHandler<TParameters, TModel>
{
    public override Type HandlerType => typeof(THandler);

    public override Type? ParametersType => typeof(TParameters);

    public override async Task<object?> LoadAsync(
        object? parameters,
        WidgetLoadContext context,
        CancellationToken cancellationToken
    ) =>
        await context
            .HttpContext.RequestServices.GetRequiredService<THandler>()
            .LoadAsync((TParameters)parameters!, context, cancellationToken);
}

// Stands in for a handler that implements no widget handler interface, or several, until the widgets are checked
internal sealed class UnusableWidgetLoader(Type handlerType, IReadOnlyList<Type> interfaces)
    : WidgetLoader
{
    public override Type HandlerType => handlerType;

    public IReadOnlyList<Type> Interfaces => interfaces;

    public override Type? ParametersType => null;

    public override Task<object?> LoadAsync(
        object? parameters,
        WidgetLoadContext context,
        CancellationToken cancellationToken
    ) =>
        throw new InvalidOperationException(
            $"{handlerType.Name} can't load a widget because it doesn't implement exactly one widget handler interface."
        );
}
