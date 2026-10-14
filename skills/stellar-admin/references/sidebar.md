# Sidebar and host pages

The sidebar lists each resource and any custom links, in groups. The app's own Razor Pages and MVC views can render inside the admin layout.

## Contents

- [Resource items](#resource-items)
- [Custom links](#custom-links)
- [Groups and order](#groups-and-order)
- [Host pages in the admin layout](#host-pages-in-the-admin-layout)

## Resource items

Each resource adds an item linking to its index. Configure it with `SidebarItem`:

```csharp
admin.AddEfCoreResource<VoyagerDbContext, Booking>(resource =>
{
    resource.SidebarItem(item =>
    {
        item.Group = "Sales";
        item.Order = 1;
        item.Label = "All bookings";
    });
});
```

| Property | Default |
| --- | --- |
| `Group` | `"Resources"` |
| `Order` | `0`, ascending within the group |
| `Label` | The plural label |
| `Visible` | `true`. Hiding the item does not disable or protect the pages. |

Items for resources the user isn't authorized for are hidden automatically ([authorization](authorization.md)). Items have no icons.

## Custom links

Link to the app's own pages, MVC actions or any URL with `AddSidebarLink` (target types in `StellarAdmin.Sidebar`):

```csharp
using StellarAdmin.Sidebar;

admin.AddSidebarLink("Departures this week", SidebarLinkTarget.Page("/Admin/Departures"), link =>
{
    link.Group = "Sales";
    link.Order = 3;
    link.RequireAuthorization(policy => policy.RequireRole("Sales"));
});

admin.AddSidebarLink("Invoices", SidebarLinkTarget.Action("Index", "Invoices"));

admin.AddSidebarLink("Help centre", SidebarLinkTarget.Url("https://help.voyager.travel"), link =>
{
    link.Group = "Help";
    link.OpenInNewTab = true;
});
```

- `SidebarLinkTarget.Page(page, area)` and `Action(action, controller, area)` resolve outside any area unless an area is given, so they reach the host app's pages. `Url(url)` takes absolute URLs and `~/` paths.
- Link builder: `Group`, `Order`, `OpenInNewTab`, `RequireAuthorization(...)`.
- `link.RequireAuthorization` hides the link from users who don't satisfy it. It **does not protect the destination**; protect host pages with `[Authorize]`.
- Links appear in the command palette too.

## Groups and order

- Items with the same group label merge into one group, whether they come from resources or links. A link without a `Group` sits at the top level, with no heading.
- Within a group, items sort by `Order`. With equal orders, resource items and links don't interleave: set `Order` explicitly to mix them.
- Groups appear in the order their label is first registered. There is no group order setting, so register the resource or link of the group that should come first earlier.

## Host pages in the admin layout

A host Razor Page or MVC view renders inside the admin layout (sidebar, header, theme, command palette) when it sets the admin layout. It keeps its own URL, routing and authorization; it is not added to the admin area.

```razor
@* Pages/Admin/Departures.cshtml *@
@page
@model DeparturesModel
@using StellarAdmin
@using StellarAdmin.UI
@addTagHelper *, StellarAdmin.UI
@{
    Layout = StellarAdminLayouts.Default;
    ViewData["Title"] = "Departures this week";
}

<sa-page-container>
    <sa-page-header>
        <sa-page-header-title>Departures this week</sa-page-header-title>
    </sa-page-header>
    <sa-table>
        <sa-table-header>
            <sa-table-row>
                <sa-table-head>Reference</sa-table-head>
                <sa-table-head>Destination</sa-table-head>
                <sa-table-head>Departs</sa-table-head>
            </sa-table-row>
        </sa-table-header>
        <sa-table-body>
            @foreach (var booking in Model.Departures)
            {
                <sa-table-row>
                    <sa-table-cell>@booking.Reference</sa-table-cell>
                    <sa-table-cell>@booking.Destination</sa-table-cell>
                    <sa-table-cell>@booking.DepartureDate.ToString("d MMM yyyy")</sa-table-cell>
                </sa-table-row>
            }
        </sa-table-body>
    </sa-table>
</sa-page-container>
```

```csharp
[Authorize(Roles = "Sales")]
public class DeparturesModel(VoyagerDbContext db) : PageModel
{
    public List<Booking> Departures { get; private set; } = [];

    public async Task OnGetAsync()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        Departures = await db.Bookings
            .Where(booking => booking.DepartureDate >= today && booking.DepartureDate < today.AddDays(7))
            .OrderBy(booking => booking.DepartureDate)
            .ToListAsync();
    }
}
```

- `Layout = StellarAdminLayouts.Default` (namespace `StellarAdmin`), in the page or in a `_ViewStart.cshtml` beside a folder of admin pages.
- `ViewData["Title"]` sets the browser title (`{title} - StellarAdmin`). The sidebar link is marked active while the page is open.
- Build the body from `sa-*` components (`sa-page-container`, `sa-page-header`, `sa-card`, `sa-table`, `sa-alert`); see the stellar-admin-ui skill for their markup. The admin CSS contains only the classes its components use, so Tailwind utilities such as `p-4` or `text-gray-500` do nothing.
- Protect the page itself (`[Authorize]` on the page model, or a Razor Pages authorization convention). The admin panel's `RequireAuthorization` covers only its own routes.
- Don't register host pages as controllers in the admin area (`AddController`); that is for extending the panel's own routes.
