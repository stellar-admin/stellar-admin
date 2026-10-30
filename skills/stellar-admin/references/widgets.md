# Widgets

Widgets are blocks on the panel's home page and on resources' index, view, create and edit pages, such as stats, a chart, a list of recent records or quick links. A widget is a partial view, optionally with a handler that loads its model. The partial view renders the whole widget, including its card, heading and any controls.

## Contents

- [Adding widgets](#adding-widgets)
- [Handlers](#handlers)
- [Parameters](#parameters)
- [Inputs](#inputs)
- [Widget settings](#widget-settings)
- [The home page](#the-home-page)
- [Index pages](#index-pages)
- [View, create and edit pages](#view-create-and-edit-pages)
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
| `Placement` | `Top` | Resource pages only: `WidgetPlacement.Top` above the grid, fields or form, `Aside` in a column beside it, `Bottom` below it. See [index pages](#index-pages) and [view, create and edit pages](#view-create-and-edit-pages). |
| `ShowWhen(context => ...)` | shown | Hides the widget when the delegate returns `false`, when the page renders. |
| `RequireAuthorization(...)` | none | The same overloads as resources. A user who doesn't meet them doesn't see the widget, and its URL returns 404. |

Widgets in a row share its height, so cards line up.

## The home page

- Outside Development, or with `ShowSetupChecklist = false`, the home page shows the widgets the user can see in place of its links to the resources. A user who sees no widgets gets the links.
- In Development, the setup checklist shows above the widgets.

## Index pages

`Widgets(...)` on a resource's `Index(...)` adds widgets to its index page. The delegates get an `IndexWidgetContext<TResource>`, which adds the grid's `Search` (trimmed, or `null`) and `Scope` (the active scope's id, or `null` without scopes) to the page's context.

```csharp
admin.AddEfCoreResource<Booking>(resource =>
{
    resource.Index(index =>
    {
        index.EnableSearch(term => booking => booking.Reference.Contains(term));
        index.Widgets(widgets =>
        {
            widgets.Add<BookingTotalsHandler>("Totals", "_BookingTotalsWidget", widget =>
                widget.Parameters(context => new BookingTotalsParameters(context.Scope, context.Search)));
            widgets.Add<UpcomingDeparturesHandler>("Upcoming departures", "_UpcomingDeparturesWidget", widget =>
                widget.Placement = WidgetPlacement.Aside);
        });
    });
});
```

- `Top` widgets sit between the page header and the grid, `Bottom` widgets below the grid, both in a 12-column grid. `Aside` widgets stack in a 20rem column beside the grid once the page is about 56rem wide, and below the grid when narrower; `Span` doesn't matter there.
- The widgets render again when the search changes, and with the page when the scope changes, so `Parameters` and `ShowWhen` see the new values. Paging and sorting leave them as they are.
- The handler gets only what `Parameters` passes: to cover the records the grid lists, pass `Scope` and `Search` and apply the same filters in the handler.
- Partial views resolve from `Areas/StellarAdmin/Views/{slug}/`, then `Areas/StellarAdmin/Views/Shared/`, then `Views/Shared/`. A partial used on the home page and an index page goes in `Shared`.
- A widget without a handler gets the `IndexWidgetContext<TResource>` as its model.
- Index widgets load from `{slug}/widget/{name}`, which the resource's authorization covers as well as the widget's own.

## View, create and edit pages

`Widgets(...)` on `AllowView(...)`, `AllowCreate(...)` and `AllowEdit(...)` adds widgets to those pages. Each page has its own widgets, and a name only needs to be unique on its page. On the view and edit pages, the delegates get a `RecordWidgetContext<TModel>`, which adds the record's `Key` and the page's model as `Record`; `TModel` is the page's model, so a page with its own model and handler gets that model. On the create page, they get a `CreateWidgetContext<TModel>`, which adds nothing to the page's context, since there's no record yet.

```csharp
admin.AddEfCoreResource<Customer>(resource =>
{
    resource.AllowView(view => view
        .Fields(fields => fields.Add(customer => customer.Name))
        .Widgets(widgets =>
            widgets.Add<LoginActivityHandler>("Login activity", "_LoginActivityWidget", widget =>
            {
                widget.Parameters(context => new LoginActivityParameters(context.Record.Id));
                widget.ShowWhen(context => context.Record.LastLoginAt is not null);
            })));

    resource.AllowEdit(edit => edit
        .Fields(fields => fields.Add(customer => customer.Name))
        .Widgets(widgets =>
            widgets.Add<UpcomingTripHandler>("Upcoming trip", "_UpcomingTripWidget", widget =>
            {
                widget.Placement = WidgetPlacement.Aside;
                widget.Parameters(context => new UpcomingTripParameters(context.Record.Id));
            })));
});
```

- `Top` widgets sit between the page header and the fields or form, `Bottom` widgets below them, both in a 12-column grid. `Aside` widgets stack in a 20rem column beside the fields or form once the page is about 56rem wide, and below them when narrower. Beside a create or edit form, the column stays in view as the form scrolls.
- The widgets are outside the form, so `<sa-widget-form>` works in them and their controls aren't submitted with the form.
- When an edit form is shown again because its values aren't valid, `Record` has the submitted values.
- Pass the record's key, not the record, in `Parameters`: they travel in the widget's URL. The handler loads what it needs.
- Partial views resolve as on index pages, from `Areas/StellarAdmin/Views/{slug}/` first. A widget without a handler gets the page's context as its model, so it can show the record without a handler.
- These widgets load from `{slug}/widget/{name}?page=view` (or `create`, `edit`), which the resource's authorization covers as well as the widget's own. Another page's parameters are refused.

## Mistakes

These are [configuration problems](configuration-problems.md): the widget is left out, or the setting is reset, and the problem is listed and logged.

- A blank name or partial name, or a name another widget on the page has (names are compared ignoring case).
- A handler that implements no widget handler interface, or more than one.
- `Parameters` that don't match the handler, or are missing.
- `Parameters` or `RefreshInterval` on a widget without a handler (ignored).
- A `Placement` other than `Top` on the home page (shown in the grid).
- Widgets on a create or edit form that `UseInlineRow()` puts in the index grid, which has no create or edit page (left out).
- A `Span` outside 1 to 12 (12), or a `RefreshInterval` under one second (no refresh).
