# Customization

Add to or replace generated pages with Razor views in the app, change labels for every resource, and override the panel's editor and display templates. For your own pages in the admin layout, see [sidebar](sidebar.md#host-pages-in-the-admin-layout).

## Contents

- [View imports](#view-imports)
- [Overriding a page](#overriding-a-page)
- [Home page](#home-page)
- [Add content to a generated page](#add-content-to-a-generated-page)
- [Page tag helpers and slots](#page-tag-helpers-and-slots)
- [Global labels](#global-labels)
- [Template overrides](#template-overrides)

## View imports

The panel's own `_ViewImports.cshtml` is compiled into the package and doesn't apply to the app's views. Any app view under `Areas/StellarAdmin/Views/` (overrides, partials, display templates) needs the app's own:

```razor
@* Areas/StellarAdmin/Views/_ViewImports.cshtml *@
@using StellarAdmin
@using StellarAdmin.UI
@using StellarAdmin.Areas.StellarAdmin
@using StellarAdmin.Areas.StellarAdmin.ViewModels
@using Voyager.Models
@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers
@addTagHelper *, StellarAdmin.UI
@addTagHelper *, StellarAdmin
```

Without it, `<sa-index-page>` and other tags are emitted as literal text: the page builds and returns 200 but shows nothing where the tag was. Check the HTML for raw `<sa-` tags after adding a view. The panel's `_ViewStart.cshtml` still applies, so overrides get the admin layout automatically.

## Overriding a page

Add a view named after the action in a folder named by the resource's **slug**: `Areas/StellarAdmin/Views/Bookings/Index.cshtml` for `/stellaradmin/bookings` (the match ignores case). The folder follows the slug, not the type name, so a resource registered as `"reservations"` uses `Views/Reservations/`.

| Page | App view | Model | Shared fallback |
| --- | --- | --- | --- |
| Index | `Areas/StellarAdmin/Views/{slug}/Index.cshtml` | `ResourceIndexPageViewModel<TResource>` | `ResourceIndex.cshtml` |
| Create | `Areas/StellarAdmin/Views/{slug}/Create.cshtml` | `ResourceFormPageViewModel` | `ResourceCreate.cshtml` |
| Edit | `Areas/StellarAdmin/Views/{slug}/Edit.cshtml` | `ResourceFormPageViewModel` | `ResourceEdit.cshtml` |
| View | `Areas/StellarAdmin/Views/{slug}/View.cshtml` | `ResourceViewPageViewModel` | `ResourceView.cshtml` |

- Models are in `StellarAdmin.Areas.StellarAdmin.ViewModels`. Use these public types, not the interfaces in `.Internal`.
- Lookup is normal MVC view lookup, so `Areas/StellarAdmin/Views/Shared/ResourceIndex.cshtml` overrides the index of every resource.
- Override to add to a page; keep `<sa-index-page>`, `<sa-form-page>` or `<sa-view-page>` to render the standard content rather than rebuilding grids and forms.

## Home page

In Development, the panel's home page (`/stellaradmin`) is a setup checklist: it shows which of resources, protection, name and logo, user menu, theme and sidebar the app has configured, with the code and instructions for a coding agent for each step. In other environments it links to the resources the user can open, or says the panel has no pages yet.

Turn the checklist off to show the same page as other environments in Development:

```csharp
builder.Services.AddStellarAdmin(admin =>
{
    admin.ConfigureHomePage(home => home.ShowSetupChecklist = false);
});
```

To replace the home page in every environment, add `Areas/StellarAdmin/Views/Home/Index.cshtml` to the app. Its model is internal, so don't declare `@model`; inject services or use a view component for the page's data. It needs the [view imports](#view-imports) like any other override.

## Add content to a generated page

A summary above the Bookings list:

```razor
@* Areas/StellarAdmin/Views/Bookings/Index.cshtml *@
@model ResourceIndexPageViewModel<Booking>
@using Microsoft.EntityFrameworkCore
@inject Voyager.Data.VoyagerDbContext Db
@{
    ViewData["Title"] = Model.Title;
    var pending = await Db.Bookings.CountAsync(booking => booking.Status == BookingStatus.Pending);
}

<sa-page-container>
    <sa-alert>
        <sa-alert-title>@pending @(pending == 1 ? "booking is" : "bookings are") awaiting confirmation</sa-alert-title>
    </sa-alert>
</sa-page-container>

<sa-index-page model="Model" />
```

- `Model.Items` holds only the current page of rows, so compute totals with your own query, as above.
- `sa-index-page` has no slot between its header and the grid; content goes above or below the whole page, or into the header action slots.
- Set `ViewData["Title"]` yourself; the built-in view sets it to `Model.Title`.
- Search, scopes, sorting and paging keep working: they replace only the grid.
- Don't put page-level content in row details; those render once per row.

## Page tag helpers and slots

| Tag | Attributes | Slots |
| --- | --- | --- |
| `<sa-index-page>` | `model`, `view`, `class` | `pre-page-actions`, `post-page-actions` (beside the Create button) |
| `<sa-form-page>` | `model`, `view`, `class` | `pre-form-fields`, `post-form-fields`, `form-leading-actions`, `pre-form-actions`, `post-form-actions` |
| `<sa-view-page>` | `model`, `view`, `class` | `pre-page-actions`, `post-page-actions`, `pre-view-fields`, `post-view-fields` |

Fill a slot with `<sa-slot-content>` inside the tag:

```razor
@model ResourceViewPageViewModel
@{ ViewData["Title"] = Model.Title; }

<sa-view-page model="Model">
    <sa-slot-content name="post-page-actions">
        <sa-button-link href="@($"/invoices/{Model.EntityId}")" variant="ButtonVariant.Outline">Invoice</sa-button-link>
    </sa-slot-content>
    <sa-slot-content name="post-view-fields">
        <sa-card><sa-card-content>Payment history goes here.</sa-card-content></sa-card>
    </sa-slot-content>
</sa-view-page>
```

The standard edit view fills `form-leading-actions` with the Delete button; an Edit override that fills that slot replaces it. `view="_MyIndexPage"` renders the tag through your own partial instead of the built-in one.

## Global labels

Change label patterns for every resource with `ConfigureResourceLabels`; each callback receives a `ResourceLabelContext` with `SingularLabel` and `PluralLabel`. Page-level settings (`create.Title`, `index.Title`) still win.

```csharp
admin.ConfigureResourceLabels(labels =>
{
    labels.Index(index =>
    {
        index.CreateLabel = resource => $"New {resource.SingularLabel.ToLower()}";
        index.SearchPlaceholder = resource => $"Find {resource.PluralLabel.ToLower()}";
    });
    labels.Create(create => create.SubmitLabel = _ => "Save");
    labels.Edit(edit => edit.SubmitLabel = _ => "Save changes");
    labels.Delete(delete => delete.ConfirmLabel = _ => "Yes, delete");
    labels.Lookup(lookup => lookup.ChooseLabel = context => $"Select {context.FieldLabel}");
});
```

| Group | Labels |
| --- | --- |
| `Index` | `Title`, `CreateLabel`, `EditLabel`, `ViewLabel`, `DeleteLabel`, `SearchPlaceholder` |
| `Create`, `Edit` | `Title`, `SubmitLabel`; `Create` also has `AddAnotherLabel` |
| `View` | `Title` |
| `Delete` | `Title`, `Message`, `ConfirmLabel`, `CancelLabel` |
| `Lookup` | `ChooseLabel`, `CreateLabel`, `NoneText`, `LoadMoreLabel`, `NoResultsTitle` and the other lookup texts (`LookupLabelContext`) |
| `Sheet` | `SaveErrorDescription`, `ErrorTitle`, `ErrorDescription`, `RetryLabel` (plain strings) |

This is the only localization mechanism; there is no `IStringLocalizer` integration. Field labels come from `[Display(Name)]`, which supports resource types.

## Template overrides

Add a view at the same path to replace the panel's markup everywhere:

| What | Path |
| --- | --- |
| A built-in editor | `Areas/StellarAdmin/Views/Shared/EditorTemplates/Editors/{Name}.cshtml` (`TextInput`, `Select`, `RadioGroup`, ...) |
| A built-in display, in forms and view pages | `Areas/StellarAdmin/Views/Shared/DisplayTemplates/Displays/{Name}.cshtml` |
| A built-in display, in index cells | `Areas/StellarAdmin/Views/Shared/GridDisplayTemplates/Displays/{Name}.cshtml` |
| A row details partial | `Areas/StellarAdmin/Views/{slug}/_Name.cshtml` or `Shared/` |

Start from the behaviour you need rather than copying internals; the built-in templates use internal view data that may change between versions. The sidebar's view is not a supported extension point.
