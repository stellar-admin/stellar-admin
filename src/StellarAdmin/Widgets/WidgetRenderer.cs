using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using StellarAdmin.Areas.StellarAdmin.ViewModels.Internal;
using AuthorizationMetadata = StellarAdmin.Infrastructure.Authorization.AuthorizationMetadata;

namespace StellarAdmin.Widgets;

// Renders a page's widgets, and loads one widget for its own request. The page decides which widgets the user sees
// and runs their delegates; the widget's request checks its authorization again and binds the parameters the page
// put in its URL. createUrl makes the URL of a widget's request from its name and protected parameters.
internal sealed class WidgetRenderer(WidgetParameters parameters, ILogger<WidgetRenderer> logger)
{
    public const string ViewName = "_Widget";

    public async Task<IActionResult> LoadAsync(
        Controller controller,
        IReadOnlyList<WidgetOptions> widgets,
        string page,
        string? name,
        Func<string, string?, string> createUrl,
        CancellationToken cancellationToken
    )
    {
        var httpContext = controller.HttpContext;
        if (
            widgets.FirstOrDefault(widget =>
                string.Equals(widget.Name, name, StringComparison.OrdinalIgnoreCase)
            )
                is not { Loader: { } loader } widget
            || !await AuthorizationMetadata.AuthorizeAsync(
                widget.AuthorizationMetadata,
                httpContext
            )
        )
        {
            return controller.NotFound();
        }

        var payload = httpContext.Request.Query[WidgetParameters.QueryName].ToString();
        object? values = null;
        if (loader.ParametersType is { } type)
        {
            (var bound, values) = await parameters.BindAsync(
                controller.ControllerContext,
                page,
                widget.Name,
                type,
                payload
            );
            if (!bound)
            {
                return Failed(controller, widget, createUrl(widget.Name, payload), 400);
            }

            payload = parameters.Protect(page, widget.Name, type, values);
        }

        var url = createUrl(widget.Name, loader.ParametersType is null ? null : payload);
        object? model;
        try
        {
            model = await loader.LoadAsync(
                values,
                new(httpContext, widget.Name),
                cancellationToken
            );
        }
        catch (Exception exception) when (!cancellationToken.IsCancellationRequested)
        {
            LogFailure(widget, exception);

            return Failed(controller, widget, url, 500);
        }

        // The widget has nothing to show, so its cell goes
        if (model is null)
        {
            controller.Response.Headers["HX-Reswap"] = "delete";

            return controller.Content("");
        }

        return controller.PartialView(
            ViewName,
            new WidgetViewModel(widget.Name, widget.PartialName, widget.Span, WidgetState.Loaded)
            {
                Model = model,
                RefreshInterval = widget.RefreshInterval,
                Url = url,
            }
        );
    }

    // The widgets the user may see, in order. Handlers of inline widgets run one at a time, since they share the
    // request's scoped services.
    public async Task<IReadOnlyList<WidgetViewModel>> RenderAsync(
        IReadOnlyList<WidgetOptions> widgets,
        WidgetContext context,
        string page,
        Func<string, string?, string> createUrl,
        CancellationToken cancellationToken
    )
    {
        var rendered = new List<WidgetViewModel>();
        foreach (var widget in widgets)
        {
            if (
                !await AuthorizationMetadata.AuthorizeAsync(
                    widget.AuthorizationMetadata,
                    context.HttpContext
                )
                || widget.ShowWhen?.Invoke(context) == false
            )
            {
                continue;
            }

            if (widget.Loader is not { } loader)
            {
                rendered.Add(
                    new(widget.Name, widget.PartialName, widget.Span, WidgetState.Loaded)
                    {
                        Model = context,
                    }
                );

                continue;
            }

            var values = widget.Parameters?.Invoke(context);
            var url = createUrl(
                widget.Name,
                loader.ParametersType is { } type
                    ? parameters.Protect(page, widget.Name, type, values)
                    : null
            );
            if (!widget.Inline)
            {
                rendered.Add(
                    new(widget.Name, widget.PartialName, widget.Span, WidgetState.Loading)
                    {
                        Url = url,
                    }
                );

                continue;
            }

            if (
                await LoadInlineAsync(widget, loader, values, context, url, cancellationToken) is
                { } inline
            )
            {
                rendered.Add(inline);
            }
        }

        return rendered;
    }

    private static PartialViewResult Failed(
        Controller controller,
        WidgetOptions widget,
        string url,
        int statusCode
    )
    {
        var result = controller.PartialView(
            ViewName,
            new WidgetViewModel(widget.Name, widget.PartialName, widget.Span, WidgetState.Failed)
            {
                Url = url,
            }
        );
        result.StatusCode = statusCode;

        return result;
    }

    private async Task<WidgetViewModel?> LoadInlineAsync(
        WidgetOptions widget,
        WidgetLoader loader,
        object? values,
        WidgetContext context,
        string url,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var model = await loader.LoadAsync(
                values,
                new(context.HttpContext, widget.Name),
                cancellationToken
            );

            return model is null
                ? null
                : new(widget.Name, widget.PartialName, widget.Span, WidgetState.Loaded)
                {
                    Model = model,
                    RefreshInterval = widget.RefreshInterval,
                    Url = url,
                };
        }
        catch (Exception exception) when (!cancellationToken.IsCancellationRequested)
        {
            LogFailure(widget, exception);

            return new(widget.Name, widget.PartialName, widget.Span, WidgetState.Failed)
            {
                Url = url,
            };
        }
    }

    private void LogFailure(WidgetOptions widget, Exception exception) =>
        logger.LogError(
            exception,
            "The {Widget} widget couldn't load: {Handler} threw an exception.",
            widget.Name,
            widget.Loader?.HandlerType.Name
        );
}
