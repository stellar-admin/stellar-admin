# Sidebar branding and user menu

Status: implemented and evaluated (2026-10-15). API and visuals agreed in conversation; see [Implementation notes](#implementation-notes) for where the code differs from the design below.

The admin panel's sidebar view (`src/StellarAdmin/Areas/StellarAdmin/Views/Shared/Components/Sidebar/Default.cshtml`) hard-codes the header ("Voyager Travel / Admin Console" with a compass tile) and a placeholder user menu ("Amelia Hart" with Account, Billing, Notifications and Log out items that do nothing). The layout (`_Layout.cshtml`) hard-codes "StellarAdmin" in the browser title, the description meta and the app header. This plan makes both configurable. It picks up the "Sidebar branding and user menu" item from the [admin panel backlog](admin-panel-backlog.md).

Prototype: `sandbox/html/sidebar-branding.html` (assets in `sandbox/html/sidebar-branding/`), each configuration rendered beside its builder code, light and dark.

## API

Two root sections on `StellarAdminBuilder`, both `Configure*` because the header and the user menu exist without the call.

```csharp
builder.Services.AddStellarAdmin(admin =>
{
    admin.ConfigureBranding(branding =>
    {
        branding.Title = "Voyager Travel";              // default: prettified application name, else "Admin"
        branding.Subtitle = "Admin Console";            // default: null
        branding.Logo = BrandingLogo.Icon("compass");   // default: null, no logo
        // or BrandingLogo.Image("~/img/logo.svg"[, "~/img/logo-dark.svg"])
        // or BrandingLogo.Wordmark("~/img/wordmark.svg"[, "~/img/wordmark-dark.svg"])
        branding.Href = "~/admin";                      // default: the admin panel root
    });

    admin.ConfigureUserMenu(menu =>
    {
        menu.NameClaimType = "name";                    // default: ClaimTypes.Name, then "name"
        menu.DescriptionClaimType = "email";            // default: ClaimTypes.Email, then "email"
        menu.AvatarClaimType = "picture";               // default: "picture"; initials when absent

        // default: [UserMenuItem.Logout()] with the log-out icon
        menu.Items.Insert(0, UserMenuItem.Page("Account", "/Account/Manage/Index", "Identity", item => item.Icon = "badge-check"));
        menu.Items.Insert(1, UserMenuItem.Separator());
        // or replace the list: menu.Items = [UserMenuItem.Url("Help", "https://help.voyager.travel"), UserMenuItem.Separator(), UserMenuItem.Logout()];

        // the built-in logout endpoint's behaviour
        menu.Logout(logout =>
        {
            logout.AuthenticationSchemes.Add(CookieAuthenticationDefaults.AuthenticationScheme);
            logout.AuthenticationSchemes.Add(OpenIdConnectDefaults.AuthenticationScheme);
            logout.RedirectPath = "~/";                // default: the admin panel root
        });
    });
});
```

## Logo hierarchy

The three logo kinds are exclusive, so they are one property with a closed class hierarchy rather than three properties checked against each other.

```csharp
public abstract class BrandingLogo
{
    private protected BrandingLogo() { }

    public static IconBrandingLogo Icon(string name);
    public static ImageBrandingLogo Image(string path);
    public static ImageBrandingLogo Image(string path, string darkPath);
    public static WordmarkBrandingLogo Wordmark(string path);
    public static WordmarkBrandingLogo Wordmark(string path, string darkPath);
}

public sealed class IconBrandingLogo : BrandingLogo { public string Name { get; } }
public sealed class ImageBrandingLogo : BrandingLogo { public string Path { get; } public string? DarkPath { get; } }
public sealed class WordmarkBrandingLogo : BrandingLogo { public string Path { get; } public string? DarkPath { get; } }
```

- The `private protected` constructor closes the hierarchy to this assembly. The subtypes are public so an overriding `_SidebarBrand.cshtml` can switch on them.
- `Icon`: an icon pack name, rendered in an accent tile beside the text. `Image`: a square image replacing the tile, beside the text. `Wordmark`: one image replacing the mark and the text.
- The dark path is an overload, not an optional parameter. `Image` takes one too, for logos that need a light version on dark.

## Branding rules

- `Title` is the one source for the sidebar header, the browser title suffix (`Products - Voyager Travel`), the description meta and the app header text. It is the wordmark's alt text.
- The default title is `IHostEnvironment.ApplicationName` made readable: split on `.`, `_` and `-` and between camel-case words (`Voyager.Admin` → "Voyager Admin", `AdminPlayground` → "Admin Playground"). An empty name falls back to "Admin".
- No logo by default (`Logo` is null): an unconfigured header is the title alone.
- A `Wordmark` logo with a `Subtitle` fails options validation at startup; the wordmark replaces the text. The hierarchy rules out every other conflict.
- A wordmark is 7 units high, its width follows the image up to the sidebar's width, left aligned. Dark mode is a `.dark` class, which `<picture>` media queries cannot follow, so both images render and CSS shows one; without a dark path the light image shows in both modes. The same applies to `Image`.
- The sidebar collapses off-canvas, so a wordmark needs no square fallback. Icon collapse would need one.

## User menu rules

- The footer renders only for an authenticated user. Anonymous requests and apps without authentication get no footer.
- Name, description and picture come from claims. The avatar is `sa-avatar`: the picture claim's image, or initials from the name as the fallback. A missing description claim drops the second line.
- The menu depends on ASP.NET Core authentication being in place; document that in the consumer skill and docs.
- `Items` is an `IList<UserMenuItem>` seeded with `UserMenuItem.Logout()` (icon `log-out`), so the default menu is Log out alone. Items render in list order; there is no ordering setting and log out has no fixed position.
- Separators render as given; leading, trailing or doubled separators are not trimmed.
- Only the logout item uses the destructive variant.

## User menu item hierarchy

Menu items are a closed hierarchy like `BrandingLogo`.

```csharp
public abstract class UserMenuItem
{
    private protected UserMenuItem() { }

    public static UrlUserMenuItem Url(string text, string url);
    public static PageUserMenuItem Page(string text, string page);
    public static PageUserMenuItem Page(string text, string page, string area);
    public static ActionUserMenuItem Action(string text, string action, string controller);
    public static ActionUserMenuItem Action(string text, string action, string controller, string area);
    public static SeparatorUserMenuItem Separator();
    public static LogoutUserMenuItem Logout();              // posts to the built-in endpoint
    public static LogoutUserMenuItem Logout(string path);   // posts to a host endpoint
    // Each factory except Separator() also has an overload with a last Action<UserMenuItemBuilder> configure.
}

public sealed class UserMenuItemBuilder
{
    public string? Icon { set; }
}

public sealed class UrlUserMenuItem : UserMenuItem
{
    public string Text { get; }
    public string Href { get; }
    public string? Icon { get; }
}
// PageUserMenuItem (PageName, Area), ActionUserMenuItem (ActionName, ControllerName, Area) and
// LogoutUserMenuItem (Path) follow the same shape; SeparatorUserMenuItem has no members.
```

- The link factories mirror `SidebarLinkTarget`'s `Url`, `Page` and `Action` overloads rather than wrapping one.
- Item settings go in a configure callback, as for `AddSidebarLink(label, target, link => ...)` (decision 21): `UserMenuItem.Page("Account", "/Account/Manage/Index", "Identity", item => item.Icon = "badge-check")`. The item stays one expression inside `Insert` or a collection expression, the items are read-only values, and later settings (such as authorization) join `UserMenuItemBuilder` without new methods. `Separator()` has no callback, so a separator cannot take an icon. `item.Icon = null` removes an icon.
- Items have no icon unless set, except logout items: both `Logout` factories start with `log-out`, so the seeded item and one added back after replacing the list look the same.
- The subtypes are public so an overriding `_SidebarUserMenu.cshtml` can switch on them.

## Log out

- Ships by default, so it is configured through the bare-noun `menu.Logout(...)`, not an `Enable*` verb.
- Named "Logout" throughout the public API (`menu.Logout`, `UserMenuItem.Logout`, `{panel}/logout`), the term .NET developers know from Identity UI; the item text is "Log out". The endpoint still calls ASP.NET Core's `SignOutAsync`.
- The built-in endpoint is a POST at `{panel}/logout`, mapped with the panel's routes and protected by antiforgery. The menu item is a form posting to it.
- No schemes listed: sign out of the default authenticate scheme, the one that signed the user in. Not `HttpContext.SignOutAsync()`: the default sign-out scheme falls back to the default sign-in scheme, which with ASP.NET Core Identity is the external cookie, so the user would stay signed in. Schemes listed: signed out in order, as MVC's `SignOutResult` does; local cookies first, a remote scheme such as OpenID Connect last, since its handler redirects to the provider's end-session endpoint and carries `RedirectPath` as the post-logout return.
- The cookie handler redirects only when the request path is its own `LogoutPath`, so the endpoint redirects to `RedirectPath` itself when no handler has redirected.
- `menu.Logout(...)` configures the endpoint only (`AuthenticationSchemes`, `RedirectPath`); where the item sits and whether it shows is the `Items` list's job.
- `UserMenuItem.Logout(path)` points the form at a host endpoint instead (e.g. Identity UI's logout, which also clears the external cookies). It always POSTs with the antiforgery token. `RedirectPath` does not apply; the host endpoint decides where to go. A host whose logout is a GET uses a `UserMenuItem.Url` item instead; document this rather than adding a method setting.
- Removing the logout item from `Items` hides it; the endpoint stays mapped, which is harmless.

## Escape hatch

Split the sidebar view into `_SidebarBrand.cshtml` and `_SidebarUserMenu.cshtml` under `Areas/StellarAdmin/Views/Shared/`. A host replaces either by placing a file at the same path (RCL view override), without copying the navigation rendering. The partials' models carry the resolved branding and user.

## Work

1. Options and builders: `BrandingOptions`/`BrandingBuilder` with the `BrandingLogo` hierarchy, `UserMenuOptions`/`UserMenuBuilder` with the `UserMenuItem` hierarchy and the seeded `Items` list, `LogoutOptions`/`LogoutBuilder`; options validation for a wordmark with a subtitle; prettified default title.
2. Views: the two partials, the layout's title, description and app header from `Title`; remove the placeholder content. Brand and wordmark CSS from the prototype goes to the panel's client CSS.
3. Logout endpoint in `MapStellarAdmin`, with HTTP integration tests (default scheme, several schemes, redirect fallback, antiforgery rejection).
4. Unit tests for the builders, defaults (including the seeded logout item), item icons, title prettifying and validation.
5. AdminPlayground configuration.
6. Consumer skills. Both admin panel skills currently tell agents this is unsupported, so they must change with the feature:
   - `skills/stellar-admin/SKILL.md`: drop "changing the sidebar's placeholder branding and user menu" from Not supported (icons on items and group ordering stay); add a task-table row for `ConfigureBranding` and `ConfigureUserMenu`; add a common mistake: the user menu needs ASP.NET Core authentication, and an anonymous panel shows no user menu.
   - `skills/stellar-admin/references/sidebar.md`: new Branding and User menu sections (title default, the three logos, dark variants, claim types, the item factories and their configure callbacks, list order and the seeded logout item, logout with several schemes, `Logout(path)` for a host endpoint, a GET logout as a `Url` item), and the two partials as the override.
   - `skills/stellar-admin/references/authorization.md`: the logout endpoint, its scheme order, and the claims the menu reads.
   - `skills/stellar-admin-setup/SKILL.md`: replace the "placeholder branding … known limitation" paragraph with a setup step setting `Title` (and a logo if the app has one), and a verify check that the header shows the app's name and the footer the signed-in user.
   - `build/skill-evals/admin-panel/tasks.md`: a task covering branding, a wordmark with a dark variant, an extra menu item and OpenID Connect logout; rerun the tasks after the skill changes.
   - Remove the item from the [admin panel backlog](admin-panel-backlog.md) when done.

## Implementation notes

Where the code differs from the design above, decided during implementation on 2026-10-15:

- All public types are in `StellarAdmin.Sidebar`, beside `SidebarLinkTarget`, so one `using` covers the sidebar API. The options live on the `StellarAdminOptions` instance, like sidebar links, and two internal singletons (`BrandingResolver`, `UserMenuResolver`) build the partials' models.
- The wordmark-with-subtitle check runs at the end of each `ConfigureBranding` call and throws `InvalidOperationException`, rather than through options validation: the panel's options are an instance configured during registration, not an `IOptions` pipeline. It still fails at startup.
- Item properties use MVC's names, because `Action`, `Page` and `Url` would hide the static factories: `ActionUserMenuItem.ActionName`/`ControllerName`/`Area`, `PageUserMenuItem.PageName`/`Area`, `UrlUserMenuItem.Href`.
- Decision 17a (`WithIcon`) was replaced by decision 21: `WithIcon` broke the options-builder rule against `With*` methods for values a property serves, and it changed and returned the item where `With*` suggests a copy.
- The log out item's text is fixed at "Log out"; there is no setting for it. Both `Logout` factories start with the `log-out` icon.
- The name defaults to `User.Identity.Name` first, which follows the identity's configured name claim type, then the `ClaimTypes.Name` and `name` claims. A user with no name shows the description in its place, or "User".
- The endpoint is `LogoutController.Index`, a POST with `[ValidateAntiForgeryToken]` on the panel's conventional route, so it gets the panel's authorization metadata and route order: `/stellaradmin/logout`.
- The log out item is an `sa-dropdown-menu-item` (a `div` menu item) inside a POST form. The dropdown dispatches a click for the mouse and Enter/Space alike, and `stellar-admin-panel.js` submits the item's form on that click. A host endpoint path renders the form with `asp-antiforgery="true"`.
- The menu puts a separator between the user label and the first item unless the first item is a separator; other separators render as given.
- Razor passes an empty `src` for a null value, so `_SidebarUserMenu.cshtml` leaves the attribute out when there is no picture claim, for the initials fallback.
- The evaluation task (task 6) covers a wordmark with a dark variant, the role claim as description, an extra item and the logout redirect, but not OpenID Connect: the evaluation apps only reach the local package feed and cookie authentication.

## Verification

Completed on 2026-10-15:

- `dotnet build` of `StellarAdmin`, `StellarAdmin.IntegrationTests` and `AdminPlayground`: no new warnings.
- `StellarAdmin.Tests`: 166 passed, including title prettifying, the wordmark-with-subtitle check, the seeded logout item, null items and item icons through the configure callback (rerun after decision 21).
- `StellarAdmin.IntegrationTests`: 453 passed (rerun after decision 21), including `Sidebar/SidebarBrandingTests` (default title, title in the header, app header, browser title and description, icon, image, wordmark with dark variant), `Sidebar/UserMenuTests` (anonymous, default and configured claims, item order, a replaced list with a host logout path, no logout item) and `Sidebar/LogoutTests` (default authenticate scheme rather than the sign-in scheme, schemes in order with a remote redirect, local schemes with the redirect path, a missing antiforgery token rejected). `Layout/HostPageLayoutTests` now expects the prettified application name in the browser title.
- AdminPlayground on port 5206 in headless Chromium: the icon logo, title and subtitle render in light and dark. In the same page, a log out item in a form submitted on a click and on Enter from the dropdown.
- `dotnet run --project util/SkillsGenerator -- --check`: no drift.

Not done:

- The signed-in user menu was not checked in the playground browser, since signing in would write to the tracked `app.db`; the integration tests cover its markup.

### Skill evaluation rerun

Run on 2026-10-15, as in phase 3 of the [admin panel skills plan](admin-panel-skills.md): fresh run directories from `prepare.sh`, the three skills in each task's `.claude/skills/`, one fresh agent per task on Sonnet 5.5 and Haiku 4.5, and task 6 also without the skills for a baseline. Every app was graded by building, running and probing it with `probe.sh` and curl: form posts for tasks 1 and 3, and for task 6 a sign-in through `/dev-login` and a log out post with the page's antiforgery token.

The first task 6 runs were void: restores took `0.0.1-eval` from the global NuGet cache, filled by the 2026-10-14 run, so the agents got packages without the branding API. `prepare.sh` now writes a run-local `globalPackagesFolder` into the run's `nuget.config`, and task 6 was rerun on fresh packages. Tasks 1–5 kept the 2026-10-14 packages; nothing they use changed since.

| Task | Sonnet with skills | Haiku with skills | Sonnet baseline | Haiku baseline |
| --- | --- | --- | --- | --- |
| 1 Install (6) | 6 | 5: booking `CustomerId` left a number input | | |
| 2 Index (8) | 8 | 8 | | |
| 3 Forms (7) | 7 | 7 | | |
| 4 Sidebar and auth (7) | 7 | 7 | | |
| 5 View and override (5) | 5 | 5 | | |
| 6 Branding and user menu (5) | 5 | 5 | 5 | 4.5: shows roles through a `roles` claim added to the fixture's `/dev-login`, rather than the role claim |
| Total (38) | 38 | 37 | | |

| Model | Tool calls per task | Time per task |
| --- | --- | --- |
| Sonnet with skills | 8–15 | 0.9–1.6 min |
| Haiku with skills | 36–82 | 2.0–4.9 min |
| Sonnet baseline, task 6 | 17 | 1.5 min |
| Haiku baseline, task 6 | 80 | 6.1 min |

Task 6 barely separates the runs: the package's XML documentation was enough for both baselines, and the skill mainly saves Haiku time. Haiku's task 3 edit page again puts a third section in the two-column grid, so Trip and Customer are not side by side there (a pass against the written check, as in phase 3).

Product findings from the agents' reports, not acted on:

- The description shows the first matching claim only, so a user with two roles shows one. The Sonnet baseline joined the roles with an `IClaimsTransformation`.
- `UserMenuItem.Page` for a page that doesn't exist renders the item without a link and without a warning; two agents switched to `Url`.

The prototype was rendered in headless Chromium in light and dark on 2026-10-14.
