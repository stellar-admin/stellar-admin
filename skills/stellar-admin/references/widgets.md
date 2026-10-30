# Widgets

Widgets are blocks on the panel's home page, such as stats, a chart, a list of recent records or quick links. A widget is a partial view, optionally with a handler that loads its model. The partial view renders the whole widget, including its card, heading and any controls.

Widgets are only on the home page for now; resource pages don't have them yet.

## Contents

- [Adding widgets](#adding-widgets)
- [Handlers](#handlers)
- [Parameters](#parameters)
- [Inputs](#inputs)
- [Widget settings](#widget-settings)
- [The home page](#the-home-page)
- [Mistakes](#mistakes)

## Adding widgets

```csharp
using StellarAdmin.Widgets;

builder.Services.AddStellarAdmin(admin =>
{
    admin.ConfigureHomePage(home => home.Widgets(widgets =>
    {
        widgets.Add<BookingStatsHandler>("Bookings", "_BookingStatsWidget", widget => widget.Span = 8);
        widgets.Add("Quick links", "_QuickLinksWidget", widget => widget.Span = 4);
    }));
});
```

- `widgets.Add(name, partialName)` adds a widget without a handler. Its partial view renders with the page, and its model is the page's `WidgetContext` (`HttpContext`, `User`, `Services`).
- `widgets.Add<THandler>(name, partialName)` adds a widget whose handler loads the model. The page renders a placeholder, and the widget loads after the page with htmx.
- Each has an overload with a `configure` callback. Without it, `Add` returns the widget's builder.
- The name identifies the widget on the page and appears in its URL. It isn't shown: the partial view renders the heading.
- Partial views resolve like any partial of the panel: `Areas/StellarAdmin/Views/Home/`, then `Areas/StellarAdmin/Views/Shared/`, then `Views/Shared/`. Put them under `Areas/StellarAdmin/Views/Home/` with the panel's [view imports](customization.md#view-imports).

```cshtml
@* Areas/StellarAdmin/Views/Home/_BookingStatsWidget.cshtml *@
@model BookingStats
<sa-card>
    <sa-card-header>
        <sa-card-title>Bookings</sa-card-title>
        <sa-card-description>This month</sa-card-description>
    </sa-card-header>
    <sa-card-content>
        <p class="text-2xl font-semibold">@Model.Count</p>
    </sa-card-content>
</sa-card>
```

## Handlers

A handler implements `IWidgetHandler<TModel>`, or `IWidgetHandler<TParameters, TModel>` for a widget with [parameters](#parameters). It is registered as a scoped service unless the app registered it already, so it can take a `DbContext` in its constructor.

```csharp
public sealed record BookingStats(int Count);

public sealed class BookingStatsHandler(VoyagerDbContext db) : IWidgetHandler<BookingStats>
{
    public async Task<BookingStats?> LoadAsync(WidgetLoadContext context, CancellationToken cancellationToken)
    {
        var start = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

        return new(await db.Bookings.CountAsync(booking => booking.CreatedAt >= start, cancellationToken));
    }
}
```

- `WidgetLoadContext` has the request's `HttpContext`, `User` and the widget's `Name`. A lazy widget's handler runs in a request of its own, after the page.
- Returning `null` hides the widget.
- A handler that throws shows an error in the widget's place with a Try again button, and the exception is logged. The rest of the page still works.

## Parameters

`Parameters` passes values from the page to the handler. The delegate gets the page's context and returns an object of the app's own type, which is serialized into the widget's URL and protected with ASP.NET Core Data Protection, so the handler can trust it.

```csharp
public sealed record RevenueParameters(string Region, [property: WidgetInput] int Days);

public sealed class RevenueHandler(VoyagerDbContext db) : IWidgetHandler<RevenueParameters, Revenue>
{
    public async Task<Revenue?> LoadAsync(RevenueParameters parameters, WidgetLoadContext context, CancellationToken cancellationToken)
    {
        // ...
    }
}

widgets.Add<RevenueHandler>("Revenue", "_RevenueWidget", widget =>
{
    widget.Parameters(context => new RevenueParameters(Region: "Europe", Days: 30));
});
```

- The handler's `TParameters` must match the delegate's return type. A handler with parameters needs `Parameters`, and a handler without them must not have it; either mismatch leaves the widget out.
- Records with a primary constructor work. Parameters are bound back with MVC model binding, so use types that bind from strings: numbers, strings, dates, enums, Guids, and lists or nested objects of these.
- An empty string binds as `null`, so a non-nullable `string` parameter that can be empty fails MVC's implicit required check, and the widget shows its error. Declare it as `string?`.

## Inputs

A widget can have its own controls, such as a period picker. Mark the properties they change with `[WidgetInput]` (`[property: WidgetInput]` on a positional record), and put the controls in `<sa-widget-form>`, named after those properties. The form loads the widget again when a control changes. Other properties keep the values from `Parameters`, whatever the request sends.

```cshtml
@model Revenue
<sa-card>
    <sa-card-header>
        <sa-card-title>Revenue</sa-card-title>
        <sa-card-action>
            <sa-widget-form>
                <sa-select name="Days" size="SelectSize.Small" aria-label="Period">
                    <option value="30" selected="@(Model.Days == 30)">30 days</option>
                    <option value="90" selected="@(Model.Days == 90)">90 days</option>
                </sa-select>
            </sa-widget-form>
        </sa-card-action>
    </sa-card-header>
    ...
</sa-card>
```

- `<sa-widget-form>` is in the `StellarAdmin` assembly: the partial's view imports need `@addTagHelper *, StellarAdmin`.
- The handler's model carries the current value, so the partial can select it, as `Model.Days` does above.
- A value that doesn't bind, such as letters for a number, shows the widget's error.

## Widget settings

| Setting | Default | |
| --- | --- | --- |
| `Span` | 12 | Columns of the 12-column grid, 1 to 12. The grid responds to its own width: narrow, every widget spans the row; medium, spans up to 6 take half. |
| `Inline` | `false` | Runs the handler while the page renders instead of after it. Inline handlers run one after another, since they share the request's services. |
| `RefreshInterval` | none | Loads the widget again on this interval, at least one second. A failed refresh keeps what the widget showed. Widgets without a handler don't refresh. |
| `ShowWhen(context => ...)` | shown | Hides the widget when the delegate returns `false`, when the page renders. |
| `RequireAuthorization(...)` | none | The same overloads as resources. A user who doesn't meet them doesn't see the widget, and its URL returns 404. |

Widgets in a row share its height, so cards line up.

## The home page

- Outside Development, or with `ShowSetupChecklist = false`, the home page shows the widgets the user can see in place of its links to the resources. A user who sees no widgets gets the links.
- In Development, the setup checklist shows above the widgets.

## Mistakes

These are [configuration problems](configuration-problems.md): the widget is left out, or the setting is reset, and the problem is listed and logged.

- A blank name or partial name, or a name another widget on the page has (names are compared ignoring case).
- A handler that implements no widget handler interface, or more than one.
- `Parameters` that don't match the handler, or are missing.
- `Parameters` or `RefreshInterval` on a widget without a handler (ignored).
- A `Span` outside 1 to 12 (12), or a `RefreshInterval` under one second (no refresh).
