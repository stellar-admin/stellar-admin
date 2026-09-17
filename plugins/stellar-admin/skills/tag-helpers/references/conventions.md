# StellarAdmin Tag Helper conventions

Read this before writing non-trivial StellarAdmin markup. These are the rules that separate markup that works from markup that silently renders wrong.

## 1. Enums are fully-qualified in Razor

Attributes backed by a C# enum take the **fully-qualified enum value**, not a lowercase string:

```razor
<!-- correct -->
<sa-button variant="ButtonVariant.Outline" size="ButtonSize.Small">Save</sa-button>

<!-- wrong — Razor can't bind "outline" to a ButtonVariant -->
<sa-button variant="outline">Save</sa-button>
```

Each component reference lists the enum type and its allowed members. When an enum attribute is omitted, the component's documented default applies. This works because `_ViewImports.cshtml` carries `@using StellarAdmin.TagHelpers` (see `setup.md`).

## 2. Attributes are kebab-case

A bound C# property maps to a kebab-case HTML attribute: `ShowCloseButton` → `show-close-button`, `IsActive` → `is-active`.

## 3. Unlisted attributes pass straight through

A component's reference lists only the attributes it *processes*. Since these tag helpers ultimately render normal HTML elements, **any other attribute you write is forwarded to the rendered element unchanged**:

- Global HTML attributes — `id`, `style`, `title`, `hidden`.
- `data-*` and `aria-*` attributes.
- The native attributes of the rendered element: `href` / `target` on tags that render an `<a>`; `type`, `name`, `value`, `disabled`, `required`, `placeholder` on tags that render an `<input>`, `<select>` or `<textarea>`.
- Event handler attributes, and library attributes such as [htmx](https://htmx.org/) (`hx-get`, `hx-target`, …) or Alpine.js.

```razor
<!-- variant and size are processed; everything else lands on the <button> -->
<sa-button variant="ButtonVariant.Outline" size="ButtonSize.Large"
           id="save" type="submit" form="booking-form" data-testid="save-button"
           hx-post="/bookings">
    Save
</sa-button>
```

Each component reference names the element a tag renders, so you know which native attributes apply. Where a component renders more than one element and needs to be specific about where an attribute lands (Table puts `class` on the `<table>` and other attributes on its scroll container), the reference says so.

## 4. `class` is appended, not replaced

`class` is the one attribute handled differently. Each tag helper controls its styling through its own CSS classes, and the classes you supply are **appended** to them:

```razor
<sa-card class="mx-auto w-full max-w-sm">...</sa-card>
```

renders

```html
<div data-slot="card" class="sa-card group/card mx-auto w-full max-w-sm">...</div>
```

**Whether an arbitrary utility actually exists depends on the app's setup.** If the project runs its own Tailwind build, utilities like `w-full` and `bg-primary` work as usual (and out-rank the component's own rules, because Tailwind's layer order puts utilities last). If it only links the prebuilt theme stylesheet, the *only* utility classes available are the small set that ships inside that bundle — `class="bg-primary"` will silently do nothing.

So: use `class` to adjust a component rather than fighting its base styles, but if the app has no Tailwind build, add your own class and style it in your own stylesheet. See `setup.md` ("use StellarAdmin's design tokens in your own markup").

## 5. Model binding

Form tag helpers accept `asp-for` and behave like the built-in ASP.NET Core tag helpers: the `name`, `id` and `value` come from the bound property, validation state is read from `ModelState`, and labels, descriptions and placeholders are taken from the property's `[Display]` metadata.

```csharp
[Display(Name = "Email address",
         Description = "Where we send your booking confirmation",
         Prompt = "you@example.com")]   // Prompt -> placeholder
[DataType(DataType.EmailAddress)]        // DataType -> input type
[Required]
public string? Email { get; set; }
```

```razor
<sa-input asp-for="Email" />
```

See the `stellar-admin:forms` skill for the full form story.

## 6. Links and routing

Tag helpers that render an `<a>` — Link Button, Breadcrumb links, Pagination links, Tab links, Link Item, Dropdown Menu links, Sidebar menu links — accept either a raw `href` or the standard ASP.NET Core routing attributes (`asp-page`, `asp-controller` / `asp-action`, `asp-route-*`, `asp-area`, `asp-fragment` and friends). Routing attributes are resolved by the framework's own anchor tag helper, so they behave exactly as they do on a plain `<a>`.

```razor
<sa-linkbutton asp-page="/Bookings/Edit" asp-route-id="@booking.Id">Edit</sa-linkbutton>
```

## 7. Overlays open declaratively — never with `onclick`

No overlay uses a JS click handler. But **which** declarative mechanism depends on the component, and the two are not interchangeable:

### Modals — `commandfor` + `command` (Invoker Commands API)

`<sa-dialog>`, `<sa-alert-dialog>` and `<sa-sheet>` render a native `<dialog>` and are driven by the **Invoker Commands API**:

- Give the overlay an `id` (or let the tag helper auto-generate one).
- A trigger **button** carries `commandfor="<overlay-id>"` and a `command`:
  - `command="show-modal"` to open,
  - `command="close"` to close.

```razor
<sa-button variant="ButtonVariant.Outline" commandfor="edit-profile" command="show-modal">
    Open
</sa-button>

<sa-sheet id="edit-profile">
    <sa-sheet-header>
        <sa-sheet-title>Edit profile</sa-sheet-title>
    </sa-sheet-header>
    <!-- ... -->
    <sa-sheet-footer>
        <sa-button commandfor="edit-profile" command="close">Save</sa-button>
    </sa-sheet-footer>
</sa-sheet>
```

### Popovers — `popovertarget` (Popover API)

`<sa-popover>` and `<sa-dropdown-menu-content>` render a native **popover**, not a dialog. The trigger points at them with `popovertarget` — there is **no** `commandfor` / `command` here:

```razor
<sa-button variant="ButtonVariant.Outline" popovertarget="view-options">
    Configure view
</sa-button>
<sa-popover id="view-options">
    <!-- ... -->
</sa-popover>
```

`<sa-dropdown-menu>` wires this up for you: `<sa-dropdown-menu-trigger>` emits the `popovertarget` pointing at the generated `<sa-dropdown-menu-content>` id, so you just nest them and write no ids at all.

Tooltips and hover-triggered popovers use `interestfor` on the trigger instead — see `javascript.md`.

### Both mechanisms

The trigger **must be a button** — a `<sa-button>` renders one; `commandfor` or `popovertarget` on any other element does nothing. The target attribute must reference a real element `id`. Some components also take custom commands (`--toggle`, `--show`, `--hide` on Collapsible). To drive a dialog from your own code instead, use `window.stellarAdmin.dialog()` — see `javascript.md`.

> Component examples often use ids that begin with a double hyphen (`id="--dialog-intro"`). That is a docs-site convention for keeping demo ids from colliding, **not** a requirement. Use ordinary ids in your own markup. (The double hyphen on a *`command`* value is different and IS required — it marks a custom command, as in `command="--toggle"`.)

## 8. Composite components have a required tag hierarchy

Many components are a family of tags that must nest correctly (sidebar, accordion, select, dropdown menu, field groups, table). Children must sit inside their parent in the documented order — e.g. `<sa-sidebar-menu-item>` inside `<sa-sidebar-menu>` inside `<sa-sidebar-group-content>`. Follow the nesting in each component's reference; don't flatten it.

## 9. Icons: `<sa-icon name="...">`

Icons use `<sa-icon>` with a Lucide icon `name` (kebab-case), e.g. `<sa-icon name="circle-arrow-left"/>`. They render as inline `<svg>` using `currentColor`, so they take the color and size of their surroundings, and compose naturally inside buttons, menu links and badges. See `icons.md` for custom icons.

## 10. Don't hand-author `<sel-*>` or runtime `data-*` state

The `<sel-*>` web components and the `data-state` / `data-side` / `data-open` / `data-mobile` attributes are emitted by the tag helpers and driven at runtime by `stellar-admin.js`. Author only the `<sa-*>` markup; never write `<sel-*>` yourself or set the runtime `data-*` attributes by hand.

## 11. Theming, briefly

Colors, radius and other design tokens come from the linked theme stylesheet (`stellar-admin.<theme>.css`) as CSS variables, exposed as Tailwind utilities like `bg-primary`, `text-muted-foreground`, `border`. Prefer these semantic tokens over hard-coded colors so components stay consistent in light and dark mode — subject to the Tailwind-build caveat in §4. Dark mode, theme customization and menu appearance are covered by the `stellar-admin:theming` skill.
