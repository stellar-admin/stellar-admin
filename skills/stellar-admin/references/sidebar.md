# Sidebar and host pages

The sidebar lists each resource and any custom links, in groups, under a header with the panel's name and above the signed-in user's menu. The app's own Razor Pages and MVC views can render inside the admin layout.

## Contents

- [Resource items](#resource-items)
- [Custom links](#custom-links)
- [Groups and order](#groups-and-order)
- [Items from code](#items-from-code)
- [Branding](#branding)
- [User menu](#user-menu)
- [Log out](#log-out)
- [Replacing the header or user menu](#replacing-the-header-or-user-menu)
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
        item.Icon = "plane";
    });
});
```

| Property | Default |
| --- | --- |
| `Group` | `"Resources"` |
| `Order` | None. Items with an order come first, ascending; the rest follow in the order they're added. |
| `Label` | The plural label |
| `Icon` | None. An icon name from the app's icon set (Lucide by default), shown before the label in the sidebar and command palette. |
| `Visible` | `true`. Hiding the item does not disable or protect the pages. |

Items for resources the user isn't authorized for are hidden automatically ([authorization](authorization.md)).

## Custom links

Link to the app's own pages, MVC actions or any URL with `AddSidebarLink` (target types in `StellarAdmin.Sidebar`):

```csharp
using StellarAdmin.Sidebar;

admin.AddSidebarLink("Departures this week", SidebarLinkTarget.Page("/Admin/Departures"), link =>
{
    link.Group = "Sales";
    link.Icon = "calendar";
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
- Link builder: `Group`, `Icon`, `Order`, `OpenInNewTab`, `RequireAuthorization(...)`.
- `link.RequireAuthorization` hides the link from users who don't satisfy it. It **does not protect the destination**; protect host pages with `[Authorize]`.
- Links appear in the command palette too.
- A `Page` or `Action` target that doesn't exist shows in Development as a red, disabled item with the reason ("No page /Admin/Departures"). In other environments it is hidden and a warning is logged; it is never in the command palette. User menu items behave the same way.

## Groups and order

- Items with the same group label merge into one group, whether they come from resources or links. A link without a `Group` sits at the top level, with no heading.
- Icons are optional per item, and an item without one reserves no space, so give every item in a group an icon or none.
- Within a group, items with an `Order` come first, ascending. Items without one follow in the order their resources and links are added, so writing the calls in the order you want is usually enough.
- At the top level, groups and ungrouped links sort the same way: by the group's or link's `Order`, then in the order the link or the group's first item is added.

Set a group's order, or sort its items by label, with `ConfigureSidebarGroup`. Configuring a group doesn't add it; it appears when a resource or link is in it:

```csharp
admin.ConfigureSidebarGroup("Catalogue", group => group.Order = 1);
admin.ConfigureSidebarGroup("Sales", group => group.Order = 2);
admin.ConfigureSidebarGroup("Settings", group => group.SortItemsByLabel = true);
```

Sort every group and item by label with `ConfigureSidebar`. Anything with an `Order` still comes first, and a group's `SortItemsByLabel` overrides the panel's:

```csharp
admin.ConfigureSidebar(sidebar =>
{
    sidebar.SortGroupsByLabel = true; // groups and ungrouped links
    sidebar.SortItemsByLabel = true;  // the items in every group
});
```

Labels sort ignoring case, in the current culture. Items and groups from an `ISidebarItemsProvider` follow those added with `AddEfCoreResource`, `AddResource` or `AddSidebarLink` when nothing else orders them.

## Items from code

When the items depend on the request or on data, implement `ISidebarItemsProvider` (in `StellarAdmin.Sidebar`) and register it as a service. Its items merge with the resource items and custom links, in the sidebar and the command palette:

```csharp
using StellarAdmin.Sidebar;

public sealed class RecentTripsSidebarItemsProvider(VoyagerDbContext db) : ISidebarItemsProvider
{
    public async Task<IReadOnlyList<SidebarItem>> GetItemsAsync(
        HttpContext httpContext,
        CancellationToken cancellationToken
    )
    {
        var trips = await db.Trips
            .OrderByDescending(trip => trip.UpdatedAt)
            .Take(3)
            .ToListAsync(cancellationToken);

        return
        [
            new SidebarGroupItem(
                "Recent trips",
                trips.Select(trip => new SidebarLinkItem(trip.Name, $"~/Trips/{trip.Id}") { Icon = "plane" })
            ),
        ];
    }
}

builder.Services.AddScoped<ISidebarItemsProvider, RecentTripsSidebarItemsProvider>();
```

- Return `SidebarGroupItem`s and links: `SidebarLinkItem(label, href)`, `SidebarActionLinkItem(label, controller, action, area)` and `SidebarPageLinkItem(label, page, area)`. An empty area links outside any area, and `null` keeps the current one, which is the admin panel's.
- Links take `Icon`, `Order` and `OpenInNewTab`. Groups and order work as above, so a group with the label of an existing group joins it.
- The provider runs on every admin page, so keep it fast. Hide items the user mustn't see yourself, using `httpContext.User`.

## Branding

`ConfigureBranding` sets the panel's name and the sidebar header's logo:

```csharp
using StellarAdmin.Sidebar;

admin.ConfigureBranding(branding =>
{
    branding.Title = "Voyager Travel";
    branding.Subtitle = "Admin Console";
    branding.Logo = BrandingLogo.Icon("compass");
});
```

| Property | Default |
| --- | --- |
| `Title` | The application name made readable (`Voyager.Admin` becomes "Voyager Admin"), or "Admin". It is also the browser title suffix (`Bookings - Voyager Travel`), the app header text and the description meta. |
| `Subtitle` | None. A second line under the title. |
| `Logo` | None: the header shows the title alone. |
| `Href` | The panel's home page. An absolute URL or `~/` path. |

The logo is one of three kinds, so only one can be set:

- `BrandingLogo.Icon("compass")`: an icon from the app's icon set, in an accent-coloured tile beside the title.
- `BrandingLogo.Image("~/img/logo.svg")`: a square image beside the title.
- `BrandingLogo.Wordmark("~/img/wordmark.svg")`: an image that replaces the title and subtitle, as tall as the two lines it replaces and up to the sidebar's width. The title becomes its alt text. It's centered by default. A wordmark with a `Subtitle` doesn't show the subtitle, and a configuration problem is logged.

`Image` and `Wordmark` take a second path for dark mode: `BrandingLogo.Wordmark("~/img/wordmark.svg", "~/img/wordmark-dark.svg")`. Without it, the same image shows in both modes.

A wordmark takes an optional `WordmarkLayout` last: `Center` (the default), `Start` to line it up with the menu items below, or `Fill` to stretch it to the sidebar's width, up to twice the standard height. Use `Fill` for a wide logo that looks small at the standard height.

```csharp
branding.Logo = BrandingLogo.Wordmark("~/img/wordmark.svg", WordmarkLayout.Fill);
branding.Logo = BrandingLogo.Wordmark("~/img/wordmark.svg", "~/img/wordmark-dark.svg", WordmarkLayout.Start);
```

## User menu

The sidebar footer shows the signed-in user: an avatar, their name and a second line, with a menu of items. It needs ASP.NET Core authentication and shows only for an authenticated user; there is no user menu on an anonymous panel.

```csharp
admin.ConfigureUserMenu(menu =>
{
    menu.DescriptionClaimType = ClaimTypes.Role;
    menu.Add(UserMenuItem.Page("Account", "/Account/Manage/Index", "Identity", item => item.Icon = "badge-check"));
    menu.Add(UserMenuItem.Separator());
});
```

| Property | Default |
| --- | --- |
| `NameClaimType` | The identity's name (`User.Identity.Name`), then the `ClaimTypes.Name` and `name` claims. |
| `DescriptionClaimType` | The `ClaimTypes.Email`, then `email` claim. Without the claim, the name shows alone. A configured claim type with several claims, such as `ClaimTypes.Role` for a user in two roles, shows them all: "Sales, Finance". |
| `AvatarClaimType` | `picture`, a URL. Without it, the avatar shows initials from the name. |
`menu.Add(item)` adds an item to the end of the menu, and `menu.Clear()` removes the items added so far. Both return the builder. The log out item isn't one of the items: it always shows after them ([log out](#log-out)). Item kinds, from `UserMenuItem`:

- `Url(text, url)`, `Page(text, page)`, `Page(text, page, area)`, `Action(text, action, controller)` and `Action(text, action, controller, area)`: links. Pages and actions resolve outside any area unless one is given.
- `Separator()`: a line between groups of items.

Every factory except `Separator()` has an overload with a last `item => ...` callback for the item's settings: `item.Icon = "badge-check"` adds an icon.

## Log out

The log out item shows after the other items, with the `log-out` icon. It posts a form, with an antiforgery token, to the panel's logout endpoint at `/stellaradmin/logout`. The endpoint signs out and redirects. Configure the item and the endpoint with `menu.Logout(...)`:

```csharp
admin.ConfigureUserMenu(menu => menu.Logout(logout =>
{
    logout.AddAuthenticationScheme(CookieAuthenticationDefaults.AuthenticationScheme);
    logout.AddAuthenticationScheme(OpenIdConnectDefaults.AuthenticationScheme);
    logout.RedirectPath = "~/";
}));
```

- With no schemes added, it signs out of the default authenticate scheme, the one that signed the user in: the cookie for cookie authentication, the application cookie for ASP.NET Core Identity.
- Added schemes are signed out in order. Put local cookies first and a remote scheme such as OpenID Connect last: its handler redirects to the identity provider, which returns to `RedirectPath` afterwards.
- `RedirectPath` (default: the panel's home page) is where the endpoint goes when no handler redirected.
- To use the app's own logout endpoint, such as Identity UI's (which also clears the external and two-factor cookies), set `logout.Path = "~/Identity/Account/Logout"`. The item posts there with an antiforgery token, and that endpoint decides where to go, so the schemes and `RedirectPath` don't apply.
- `logout.Icon = null` removes the icon.
- `logout.Show = false` hides the log out item. An app whose logout is a GET request hides it and adds a link instead: `menu.Add(UserMenuItem.Url("Log out", "~/logout"))`.

## Replacing the header or user menu

For a header or user menu the settings can't describe, add a view at the same path in the app, and it replaces the panel's:

- `Areas/StellarAdmin/Views/Shared/_SidebarBrand.cshtml`, with model `SidebarBrandModel` (`Title`, `Subtitle`, `Logo`, `Href`). Switch on `Logo` for `IconBrandingLogo`, `ImageBrandingLogo` or `WordmarkBrandingLogo`.
- `Areas/StellarAdmin/Views/Shared/_SidebarUserMenu.cshtml`, with model `SidebarUserMenuModel` (`Name`, `Description`, `AvatarUrl`, `Items`). It renders only for a signed-in user.

Both models are in `StellarAdmin.Sidebar`. The views need the `_ViewImports.cshtml` tag helper registrations that other admin area overrides need ([customization](customization.md)).

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
- `ViewData["Title"]` sets the browser title (`{title} - {branding title}`, e.g. `Departures this week - Voyager Travel`). The sidebar link is marked active while the page is open.
- Build the body from `sa-*` components (`sa-page-container`, `sa-page-header`, `sa-card`, `sa-table`, `sa-alert`); see the stellar-admin-ui skill for their markup. The admin CSS contains only the classes its components use, so Tailwind utilities such as `p-4` or `text-gray-500` do nothing.
- Protect the page itself (`[Authorize]` on the page model, or a Razor Pages authorization convention). The admin panel's `RequireAuthorization` covers only its own routes.
- Don't register host pages as controllers in the admin area (`AddController`); that is for extending the panel's own routes.
