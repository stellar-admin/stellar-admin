# Admin panel evaluation tasks

Six tasks for measuring the admin panel consumer skills: run once without the skills for a baseline, then again with them. Each task runs in its own copy of the Voyager app (see [README](README.md)). The **Prompt** is given to the agent verbatim; the **Checks** are for grading and are never shown to it.

Grade each check as pass, partial or fail, and record the API mistakes behind every non-pass: invented members, wrong namespaces, wrong ordering, configuration that compiles but does nothing, and workarounds where a built-in feature exists.

## Task 1: install

Starts from: `task-1` (no StellarAdmin).

**Prompt**

> Add the StellarAdmin admin panel to this app. The NuGet packages StellarAdmin and StellarAdmin.EntityFrameworkCore (version 0.0.1-eval) are available from the local feed in nuget.config. Staff should be able to list, create, edit and delete customers and bookings. Only signed-in users with the Staff role may use the admin panel. Make sure it builds and works.

**Checks**

1. Both packages referenced; the app builds.
2. `AddStellarAdmin` with `AddEfCoreResource<Customer>` and `AddEfCoreResource<Booking>`, each with index columns, `AllowCreate`, `AllowEdit` and `AllowDelete`.
3. `app.MapStellarAdmin()` mapped; authentication and authorization middleware run before it.
4. Staff-only access through `admin.RequireAuthorization(policy => policy.RequireRole("Staff"))` or `app.MapStellarAdmin().RequireAuthorization(...)`: anonymous `/stellaradmin/bookings` challenges, a signed-in user without Staff is forbidden, Staff gets 200.
5. The booking form's `CustomerId` is a choice editor (`SelectEditor` or `LookupSheetEditor` with EF `UseItems`) rather than a number input.
6. Creating and editing a booking through the forms succeeds.

## Task 2: index page

Starts from: `task-2` (StellarAdmin installed; Bookings has three plain columns).

**Prompt**

> Improve the Bookings list in the admin panel. Show the reference, customer name, destination, departure date, status and total (as currency). Show the status as a badge: confirmed bookings in the default style, cancelled in the destructive style and pending in the secondary style. Let staff search by reference or customer name, filter by status with tabs (All, Pending, Confirmed, Cancelled), sort by departure date (soonest first by default) and by total, and show 10 bookings per page.

**Checks**

1. Customer column reads through the reference (`booking => booking.Customer!.Name`) with a "Customer" title.
2. Total uses `NumberDisplay` with a currency format (or `[DisplayFormat]`).
3. Status badge per value: a display template named by `[UIHint]` placed in `GridDisplayTemplates/` (a column renders `GridDisplayTemplates/{name}`, not `DisplayTemplates/`); `BadgeDisplay` alone cannot vary the variant by value.
4. `index.EnableSearch(term => booking => ...)` covering reference and customer name.
5. `index.EnableScopes(...)` with an unfiltered "all" scope, one predicate per status and `DefaultScope`.
6. `Sortable()` on departure date and total, `DefaultSortBy(booking => booking.DepartureDate)`.
7. `index.EnablePaging(paging => paging.PageSize = 10)`.
8. The page renders, and searching, tabs, sorting and paging work.

## Task 3: forms

Starts from: `task-3` (StellarAdmin installed; Bookings has no forms).

**Prompt**

> Add create and edit pages for bookings in the admin panel. Lay the form out as two sections side by side on wide screens: "Trip" (destination, departure date, cabin, travellers) and "Customer" (customer, notes). Staff should pick the customer from a searchable list and be able to add a new customer from there without leaving the form. Show the cabin choices as selectable cards and the number of travellers as a slider. On the edit page, staff can change the status, and the booking's reference and creation time are shown read-only. New bookings get a generated reference: "VT-" followed by six random digits.

**Checks**

1. `fields.Columns(...)` with two columns at a wide breakpoint, and two `AddSection` calls.
2. Customer uses `LookupSheetEditor` with EF `UseItems<Customer, int>(...)` and `EnableCreate()` (Customers already has a create form).
3. Cabin uses `RadioGroupEditor` with `Appearance = RadioGroupAppearance.Cards`.
4. Travellers uses `SliderEditor` (range from `[Range]` or explicit).
5. Edit adds `Status` as a field, and `AddDisplayField` (or `AddDetails`) for reference and creation time, not editable fields.
6. Reference generated through `create.UseFactory(() => new Booking { Reference = ... })` or a create handler, not a hidden field or controller hack.
7. Creating a booking, including one with a customer created from the lookup, and editing it both work.

## Task 4: sidebar and authorization

Starts from: `task-4` (StellarAdmin installed).

**Prompt**

> Put Customers and Bookings in a "Sales" group in the admin panel sidebar, with Bookings first. Add a "Departures this week" page to the app that lists bookings departing in the next 7 days; it should appear in the Sales group and render inside the admin panel's layout so it looks like the rest of the admin. Only users in the Sales role may see or open Bookings and the departures page. Customers stays available to any signed-in user, and the admin panel requires sign-in. Also add a "Help centre" link to https://help.voyager.travel in a "Help" group that opens in a new tab.

**Checks**

1. `resource.SidebarItem(item => { item.Group = "Sales"; item.Order = ...; })` on both resources, Bookings ordered first.
2. `admin.AddSidebarLink("Departures this week", SidebarLinkTarget.Page(...), link => { link.Group = "Sales"; ... })`.
3. The page sets `Layout = StellarAdminLayouts.Default` and uses StellarAdmin.UI components (`sa-page-container`, `sa-page-header`, a table) with `@addTagHelper *, StellarAdmin.UI`.
4. `admin.RequireAuthorization()` for sign-in; Bookings `resource.RequireAuthorization(policy => policy.RequireRole("Sales"))`.
5. The departures link uses `link.RequireAuthorization(...)` **and** the page itself is protected (`[Authorize(Roles = "Sales")]` or a page convention); hiding the link alone does not protect it.
6. Help link via `SidebarLinkTarget.Url(...)` with `Group = "Help"` and `OpenInNewTab = true`.
7. Behaviour: a signed-in non-Sales user sees Customers but not Bookings or the departures link and is forbidden from both; a Sales user sees and opens both.

## Task 5: view page and view override

Starts from: `task-5` (StellarAdmin installed).

**Prompt**

> Add a read-only detail page for bookings in the admin panel showing every booking field plus the customer's name and email, grouped into "Trip", "Customer" and "Audit" sections. Then customise the Bookings list page: above the list, show a short summary of how many bookings are awaiting confirmation (status Pending), and keep the standard list below it.

**Checks**

1. `resource.AllowView(view => view.Fields(...))` with three `AddSection` calls, reading `booking => booking.Customer!.Name` and `.Email`.
2. A View action appears on index rows and the page renders.
3. An index override view for the resource (under `Areas/StellarAdmin/Views/`) whose model is `ResourceIndexPageViewModel<Booking>` and which renders `<sa-index-page model="Model" />` below the summary, rather than rebuilding the grid.
4. The admin area view imports register the `StellarAdmin` and `StellarAdmin.UI` tag helpers where the override needs them.
5. The summary count is correct and the list keeps search, sorting and paging.

## Task 6: branding and user menu

Starts from: `task-6` (StellarAdmin installed).

**Prompt**

> Brand the admin panel as "Voyager Travel". The designers will add a wordmark at wwwroot/img/voyager-wordmark.svg, and a version for dark mode at wwwroot/img/voyager-wordmark-dark.svg; show it at the top of the admin sidebar. In the sidebar's user menu, show each user's role under their name, add a "My bookings" link to the app's /MyBookings page above the log out option, and send users to the app's home page after they log out.

**Checks**

1. `admin.ConfigureBranding(...)` with `Title = "Voyager Travel"` and `Logo = BrandingLogo.Wordmark("~/img/voyager-wordmark.svg", "~/img/voyager-wordmark-dark.svg")`, and no `Subtitle` (a wordmark with a subtitle throws at startup).
2. `menu.DescriptionClaimType = ClaimTypes.Role`.
3. "My bookings" added as `UserMenuItem.Page("My bookings", "/MyBookings")` (or `Url`) before the seeded log out item, which is kept: `menu.Items.Insert(0, ...)`, or a replacement list that includes `UserMenuItem.Logout()`.
4. `menu.Logout(logout => logout.RedirectPath = "~/")`; no hand-written logout endpoint or view override.
5. Behaviour: signed in through `/dev-login`, the sidebar shows the wordmark images with "Voyager Travel" as their alt text, the role under the name and the "My bookings" link above Log out; Log out lands on `/`, and the user is signed out.
