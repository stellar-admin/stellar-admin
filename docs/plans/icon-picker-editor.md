# Icon display and editor

Status: phases 1 and 2 committed (db35960, a755d96); phase 3 implemented (2026-10-19), awaiting review. UX agreed in the prototype `sandbox/html/icon-field.html`, whose numbered questions are referenced below.

## Goal

Show and edit a string property that holds the name of an icon, so staff choose from the panel's icons instead of typing a name they have to know. From the Voyager Travel session feedback, where an amenity's `Icon` field was a plain text box. The field offers every registered icon by default, and an editor setting can restrict it to a curated list.

## Decisions (Jerrie, 2026-10-18 and 2026-10-19)

1. One display, `IconDisplay`, for the grid and the view page, and one editor, `IconEditor`, for forms and inline grid rows. Both are opt-in through `UseDisplay` and `UseEditor`. No convention or attribute picks them automatically.
2. Grid display (questions 1–3): the icon alone in the ink colour, with its name on hover. `ShowName = true` shows the icon and its name.
3. View page and row details (question 4): the icon with its name beside it, at text size. `ShowName = false` shows the icon alone. `ShowName` unset means the default for where the field is shown.
4. An empty value shows a dash, like other fields. A name that isn't a registered icon shows a dashed box where the icon would be, with the name on hover. When the name is shown, the dashed box is followed by the name in the muted colour.
5. The editor (questions 5 and 6) is a select-style trigger showing the current icon and name, which opens a popover with a search box, a grid of 40px icon cells and a status line. The status line names the icon under the pointer or with focus, shows the count, and has Clear when the field is optional. Search matches names and keywords. With no search, the current icon comes first. The grid renders 240 cells at a time and more as it scrolls. Keyboard: ↓ from the search box into the grid, arrow keys to move, Enter to choose, ↑ from the first row back to the search box.
6. Several registered packs (question 8) share one grid. The prefix tells them apart, so `IconOptions` needs no record of which pack an icon came from.
7. The editor's `Icons` setting restricts the field to a curated list. Unset means every registered icon.
8. The editor's layout (question 7) is `Layout` (`IconEditorLayout`: `Auto`, `Picker`, `Inline`), default `Auto`. `Auto` shows inline tiles when `Icons` has 24 or fewer icons, and the picker otherwise. The threshold is fixed. `Picker` always uses the popover, and `Inline` always uses tiles.
9. Inline tiles are a single-select `sa-toggle-group` with the outline variant, one icon-only item per icon with its name as `title` and `aria-label`.
10. In an editing grid row, a small trigger showing the icon opens the same popover.
11. Validation: a newly entered name must be a registered icon, and must be in `Icons` when the field has a curated list. A stored value that is already unknown saves unchanged, so it doesn't block editing the record's other fields.

## StellarAdmin.UI: `sa-icon-picker`

A public tag helper that works outside the admin panel, like `sa-image-upload`. StellarAdmin.UI has no endpoint.

1. The icons come from child `sa-icon` elements or from `icons-url`. Inside a picker, a child `sa-icon` also emits its name so the picker knows what to post. Outside a picker, `sa-icon` is unchanged.
2. `layout` (`IconPickerLayout`: `Picker`, `Inline`), default `Picker`. `Auto` exists only in the admin panel, which turns it into the picker's `layout`.
3. The tag helper throws when it has both children and `icons-url`, when it has neither, and when `layout` is `Inline` with `icons-url`. Inline tiles must be rendered on the server.
4. The trigger's current icon is rendered on the server from `IconOptions`, so the value shows before `icons-url` loads and a form that is never opened makes no request.
5. With `icons-url`, the picker fetches the whole set once, on first open, and searches and scrolls through it in the browser. The request has no query string, no paging and no limit. Server-side search can be added later as an opt-in without changing the payload.
6. The picker accepts only markup with an `<svg>` root and strips scripts and `on*` attributes before inserting it.

### Payload

The response to `icons-url` is documented as part of the public API:

```json
{
  "icons": [
    { "name": "wifi", "svg": "<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 24 24\" …>…</svg>", "keywords": ["internet", "wireless"] }
  ]
}
```

`name` and `svg` are required, and `keywords` is optional. The object wrapper leaves room for later fields. Complete SVG markup lets an app serve icons from any source, not only `IconOptions`.

## Admin panel endpoint

1. StellarAdmin maps the icons endpoint inside `MapStellarAdmin`, so it inherits the panel's authorization. `IconEditor` passes its URL to `icons-url` when the field has no curated list. With a curated list, it renders the icons as `sa-icon` children.
2. The URL carries a hash of the registered icons (`?v=…`), and the response is cached as immutable.
3. The endpoint builds its gzip and Brotli copies once and serves them itself, so the size doesn't depend on the app enabling response compression. Measured for this payload: Lucide is 772 KB raw, 90 KB gzip and 69 KB Brotli. Tabler outline adds 240 KB gzip.
4. The endpoint emits no keywords. `IconOptions` has none, and adding a keyword source for the packs is out of scope for now.

## API

```csharp
fields.Add(a => a.Icon, field => field.UseEditor<IconEditor>(icon =>
{
    icon.Icons = ["wifi", "waves-ladder", "coffee", "square-parking"];
    icon.Layout = IconEditorLayout.Picker;
}));

columns.Add(a => a.Icon, column => column.UseDisplay<IconDisplay>(icon => icon.ShowName = true));
displayFields.Add(a => a.Icon, field => field.UseDisplay<IconDisplay>(icon => icon.ShowName = false));
```

```html
<sa-icon-picker asp-for="Icon">
    <sa-icon name="wifi" />
    <sa-icon name="waves-ladder" />
    <sa-icon name="coffee" />
</sa-icon-picker>

<sa-icon-picker asp-for="Icon" layout="IconPickerLayout.Inline">…</sa-icon-picker>

<sa-icon-picker asp-for="Icon" icons-url="/admin/icons" />
```

## Phases (approved 2026-10-19)

Each phase stops for Jerrie's review before the next starts.

1. **StellarAdmin.UI `sa-icon-picker`.** The tag helper and `IconPickerLayout`, `sa-icon` emitting its name inside a picker, and the three throws. The CSS from the prototype, rewritten to the library's conventions. A web component for search, grid keyboard navigation, progressive rendering, fetching `icons-url` and sanitizing the SVG. The inline layout on `sa-toggle-group`. TUnit tests for the tag helper, DocsSamples demos (children, inline, `icons-url` with a sample endpoint), and a browser check of the keyboard and popover.
2. **Admin panel.** `IconDisplay` with `ShowName`, `IconEditor` with `Icons` and `Layout`, validation, the grid-row trigger, and the icons endpoint in `MapStellarAdmin` with the hash URL and precompressed copies. An icon field in the AdminPlayground editor gallery. Tests for the display, editor, validation and endpoint, including HTTP integration tests. If the sample needs a new entity property, it is shown for review first, and migrations are separate steps.
3. **Docs.** The website pages for `sa-icon-picker` (including the payload) and for the admin panel's icon field, the regenerated consumer skill references, and this plan's status.

## Phase 1 implementation (2026-10-19)

- `IconPickerTagHelper` (`sa-icon-picker`), `IconPickerLayout`, `IconPickerClassNames` (`Control`, `Item`, `Popover`, `Trigger`) and the internal `IconPickerContext` in `src/StellarAdmin.UI/TagHelpers/IconPicker/`. A child `sa-icon` registers with the picker and renders nothing itself. Duplicate child names are dropped.
- Attributes beyond the plan, following `sa-image-upload`: `placeholder` (default "Choose an icon"), `required` (default from the bound property, and an optional picker shows Clear), `disabled` (doesn't post, like a native control) and `form`. A child `sa-icon` without a name also throws.
- The inline layout renders `sa-toggle-group` markup (outline, default spacing) with one radio per icon, plus a hidden input after the radios. It posts a stored value that isn't a tile, so the value saves unchanged, and posts an empty value once the script clears an optional field.
- `sel-icon-picker` (`Client/js/web-components/sel-icon-picker.ts`) and `Client/css/components/icon-picker.css`. Besides scripts and `on*` attributes, the sanitizer also removes `foreignObject` and `javascript:` links. A failed `icons-url` request shows "Couldn't load the icons." and retries on the next open. In the search box, the first Escape clears the text (the browser's search input) and the second closes the popover.
- DocsSamples `Pages/IconPicker/`: Intro, Inline, Icons URL (with a sample `?handler=Icons` endpoint), Model Binding and Disabled. The website export entries are for phase 3.
- Verified: 16 new tag helper tests in `tests/StellarAdmin.UI.Tests/TagHelpers/IconPicker/`, and the UI unit suite passes (633). A headless Chromium check against DocsSamples passed 35/35: opening, search focus, current icon first, filtering and counts, ↓/→/↑/Enter navigation, picking, Clear, focus back to the trigger, inline clear and pick, `icons-url` first batch of 240 and more on scroll, search ranking, Escape, the required and disabled pickers, the sanitizer and keyword search on a hostile payload, and a failed request. Screenshots: default light and dark, Ops, Soft dark, and 390px mobile.

## Phase 2 implementation (2026-10-19)

- `IconDisplay` (`ShowName`), `IconEditor` (`Icons`, `Layout`) and `IconEditorLayout` (`Auto`, `Picker`, `Inline`) in `src/StellarAdmin/Resources/`, with `Displays/Icon` and `Editors/Icon` templates for forms and grids. The display shows the icon, or `.sa-icon-missing` for an unregistered name, with the name beside it or as `title` and screen-reader text. An inline row always uses the picker, with a compact trigger (`sa-icon-picker-cell-trigger`) that shows the icon and chevron and keeps the name for screen readers.
- The icons endpoint is an action on the panel's Home controller, `/stellaradmin/home/icons?v={version}`, so it adds no reserved resource slug and inherits the panel's authorization. The internal `IconCatalog` singleton builds the JSON once, versions it with the first 16 hex digits of its SHA-256, and builds the Brotli and gzip copies on first request. The current version is cached `private, max-age=31536000, immutable`, and any other version is `no-cache`. Every response has an ETag and `Vary: Accept-Encoding`, and a matching `If-None-Match` gets 304. The SVG gets `xmlns` when an icon added in code leaves it out. Measured with Lucide: 790 KB raw, 71 KB Brotli.
- Validation runs after binding in the resource controller. A changed, non-empty value must be in `Icons`, or a registered icon when there's no list. Otherwise the field shows "Choose an icon from the list." A value equal to the stored one is never checked.
- Beyond the plan: `IconEditorOptionsValidator` fails startup for an empty `Icons` list, for `Inline` without `Icons`, and for listed names that aren't registered.
- AdminPlayground: an Icon gallery under Field editors (every icon, required, 13 tiles, 32 in the picker, `Picker` and `Inline` with lists, stored unregistered values, read-only), and an in-memory Amenities resource under Commerce (grid, inline row, view page, create form). Neither is an EF entity, so no migration.
- Verified: 10 new unit tests (`IconEditor.ResolveLayout`, `IconCatalog`) and 29 new integration tests (endpoint, display, editor, validation, startup validation), plus the icons URL in the anonymous-authorization test. Full suites pass: StellarAdmin.Tests 253, IntegrationTests 576, UI.Tests 633, UI.IntegrationTests 19, EF IntegrationTests 97. A headless Chromium check of the playground passed 23/23: layouts, endpoint load, pick and save, three rejected values with their errors, grid icons and dashed box, the inline row trigger, its popover, pick and row save, and the view page.

## Phase 3 implementation (2026-10-19)

- Website: `content/docs/ui/components/icon-picker.mdx` under Forms, with the five DocsSamples demos, the `icons-url` payload and a sample endpoint, accessibility, part classes and the attribute table, plus a `sel-icon-picker` row on the JavaScript page. The website has no admin panel docs, so the admin icon field is documented in the consumer skill instead of a website page.
- Docs export: `DocsSamplesGenerator` exports the `IconPicker/` demos, downloads the sample endpoint's response to `public/demo/ui/assets/icons.json`, and rewrites `icons-url` in the exported demos to that file. Jerrie first chose the full set, but DocsSamples also registers the docs' preview packs (9,862 icons, 6.3 MB), so on 2026-10-19 he chose Lucide only: the exporter keeps `LucideIconPack`'s names, 1,838 icons, 790 KB raw, 90 KB gzip. `DocsStatic` gained the model binding demo's model. The picker demos have `min-h` wrappers (260, 440 and 210px) so their popovers fit the demo frame.
- Consumer skills: the generated `stellar-admin-ui` reference `components/icon-picker.md` (examples manifest entry, hand-written composition section) and its index row. `stellar-admin`: an Icons section in `editors.md`, `IconDisplay` rows in `columns.md` and `fields.md`, the inline row editor in `inline-editing.md`, and a task-table row in `SKILL.md`.
- Verified: `SkillsGenerator -- --check` reports no drift, and the skill validator passes (4 skills, 87 files). Website lint, type check and build pass after `pnpm examples:export`. The other website diffs are the new asset fingerprints and antiforgery tokens. A headless Chromium check of the built site shows the page with its five demos, the exported URL demo loading the 1,838 Lucide icons from `icons.json`, the popovers inside their frames, and the model binding demo's label.
