# Templated views and named slots

Writing tag helpers in ASP.NET Core is powerful, but building the HTML in C# is tedious. StellarAdmin adds two pieces so you can write your own tag helpers with Razor instead:

- **Templated tag helpers** render through a Razor view.
- **Named slots** let a caller pass content into specific spots of a tag helper's output.

## Templated tag helpers

Inherit from `StellarAdminTemplatedTagHelperBase` and name the view in `ViewName`:

```csharp
[HtmlTargetElement("docs-trip-card")]
public class TripCardTagHelper : StellarAdminTemplatedTagHelperBase
{
    [HtmlAttributeName("booking")]
    public Booking? Booking { get; set; }

    protected override string ViewName => "_TripCard";

    public TripCardTagHelper(ICompositeViewEngine viewEngine)
        : base(viewEngine) { }

    protected override object? GetViewModel()
    {
        return Booking;
    }
}
```

`GetViewModel()` is **abstract** — every subclass states its model explicitly. The view resolves like a partial view (e.g. `Pages/Shared/_TripCard.cshtml`) and is rendered with that model:

```razor
@model DocsSamples.Booking

<sa-card class="w-full max-w-sm">
    <sa-card-header>
        <sa-card-title>@Model.Destination</sa-card-title>
        <sa-card-description>Booking @Model.Id</sa-card-description>
    </sa-card-header>
    <sa-card-content>
        <div class="flex items-center justify-between text-sm">
            <sa-badge variant="BadgeVariant.Outline">@Model.Status</sa-badge>
            <span class="font-medium">@Model.Amount.ToString("C")</span>
        </div>
    </sa-card-content>
</sa-card>
```

Using it is then a single element:

```razor
<docs-trip-card booking="@Model.Booking" />
```

### Swapping the view per instance

Every templated tag helper accepts a **`view`** attribute that overrides the view for that instance, so the same tag helper can render through different markup where needed:

```razor
<docs-trip-card booking="@Model.Booking" view="_TripCardCompact" />
```

Because the view resolves like a partial, an app consuming a Razor Class Library **overrides the shipped markup by shadowing the view at the same path** — the standard ASP.NET Core mechanism, no bespoke registration.

## Slot outlets

A templated tag helper's view declares slots with `<sa-slot-outlet name="...">`. Its child content is the **fallback**, rendered when the caller doesn't fill the slot:

```razor
<sa-card-footer>
    <sa-slot-outlet name="actions">
        <sa-button size="ButtonSize.Small" variant="ButtonVariant.Outline">View details</sa-button>
    </sa-slot-outlet>
</sa-card-footer>
```

The caller fills it by placing a matching `<sa-slot-content>` among the tag helper's children:

```razor
<docs-trip-card booking="@Model.Booking">
    <sa-slot-content name="actions">
        <sa-button size="ButtonSize.Small">Rebook</sa-button>
    </sa-slot-content>
</docs-trip-card>
```

An outlet also falls back when the view is rendered as a plain partial, outside a templated tag helper.

## Named slots in any tag helper

Slots aren't limited to templated tag helpers — **any StellarAdmin tag helper can host them**. The host executes its child content (which registers the `<sa-slot-content>` children with it) and reads slots back with `TryGetNamedSlot`, rendering them wherever it chooses:

```csharp
[HtmlTargetElement("docs-booking-summary")]
public class BookingSummaryTagHelper : StellarAdminTagHelperBase
{
    [HtmlAttributeName("booking")]
    public Booking? Booking { get; set; }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var childContent = await output.GetChildContentAsync();

        output.TagName = "div";

        // Header row, with the "actions" slot rendered at the end when filled
        if (TryGetNamedSlot("actions", out var actions))
        {
            output.Content.AppendHtml(actions);
        }

        // The remaining children become the body
        output.Content.AppendHtml(childContent);
    }
}
```

The caller mixes slot content and regular children freely — `<sa-slot-content>` assigns itself to the nearest StellarAdmin ancestor and renders nothing in place. A filled slot the host never reads renders nothing at all.

## API reference

### `<sa-slot-content>`

Assigns its child content to a named slot on the nearest StellarAdmin ancestor tag helper and renders nothing in place. Assigning the same slot name twice on the same host is an error.

| Attribute | Type | Required | Description |
|-----------|------|----------|-------------|
| `name` | `string` | yes | The name of the slot to assign the content to. |

### `<sa-slot-outlet>`

Renders the content of the named slot on the hosting tag helper. When the slot is unfilled — or the view renders as a plain partial, outside a templated tag helper — it renders its own child content as the fallback instead. It renders no element of its own.

| Attribute | Type | Required | Description |
|-----------|------|----------|-------------|
| `name` | `string` | yes | The name of the slot to render. |

> These two tags have no generated component reference file: they are a composition mechanism rather than a visual component. This page is their reference.
