# Admin panel docs

Status: drafted (2026-10-24); awaiting Jerrie's review. Nothing committed.

Public documentation for the StellarAdmin admin panel (the `StellarAdmin` and `StellarAdmin.EntityFrameworkCore` packages) on the website, as a second docs root next to StellarAdmin.UI. The website has no admin panel docs today; the consumer skills in `skills/stellar-admin` and `skills/stellar-admin-setup` are the closest existing description of the feature set.

## Decisions

1. A second root tab, "StellarAdmin", at `/docs/admin`, beside "StellarAdmin.UI". The UI docs are not restructured; panel pages link to them for theming and component details.
2. Editors are documented on grouped pages, not one page per editor.
3. Features the panel doesn't have (custom and bulk actions, filters, form tabs, dashboards and the rest of the [backlog](admin-panel-backlog.md)) are left out of the navigation. Pages may say in one sentence what isn't supported where a reader would look for it.
4. This pass is prose and code only: no demos or screenshots. The panel is a full application, so its visuals need a curated Voyager Travel sample app and a capture pipeline; that is follow-up work.
5. Nothing is committed in either repository; Jerrie reviews and commits.

## Navigation

The `admin` folder is flat, with `---Section---` separators in its `meta.json` like the UI root, so URLs are `/docs/admin/{slug}`.

| Section | Page (slug) | Covers |
| --- | --- | --- |
| Getting Started | Introduction (`index`) | What the panel is, the three packages, how configuration works |
| | Installation (`installation`) | Packages, `AddStellarAdmin`, `MapStellarAdmin`, middleware order, stylesheet and theme |
| | Quick start (`quick-start`) | A first EF Core resource end to end |
| | Organizing configuration (`organizing-configuration`) | Moving resources into extension methods; registration order |
| | Agent skills (`agent-skills`) | `stellar-admin` and `stellar-admin-setup`, installing them |
| Resources | Resources (`resources`) | EF Core resources, slugs, labels, enabling pages, how configuration composes |
| | Titles and references (`titles-and-references`) | `UseTitle`, `Reference`, search and order of references |
| | Delete (`delete`) | `AllowDelete` and its settings |
| | Custom data sources (`custom-data-sources`) | `AddResource<T>`, `UseDataSource`, `UseKey`, operation results |
| Index Pages | Overview (`index-pages`) | Title, row action labels, what the index shows |
| | Columns (`columns`) | Adding columns, values through references, computed values, displays in cells |
| | Search (`search`) | `EnableSearch` |
| | Scopes (`scopes`) | Scope tabs |
| | Sorting and paging (`sorting-and-paging`) | `Sortable`, `DefaultSortBy`, `EnablePaging` |
| | Row details (`row-details`) | `EnableRowDetails`, panels, handlers |
| | Inline editing (`inline-editing`) | `UseInlineRow`, `EditsField`, adding rows, after save |
| Forms | Overview (`forms`) | `AllowCreate`, `AllowEdit`, page settings, save and add another, sharing fields |
| | Fields (`fields`) | Editable fields, display fields, details sections, read-only and disabled |
| | Layout (`form-layout`) | Columns, sections, groups |
| | Defaults and validation (`defaults-and-validation`) | `UseFactory`, `KeepForNext`, validation |
| | Custom models and handlers (`custom-models`) | Form models, create and edit handlers, fields bound by code |
| View Pages | View pages (`view-pages`) | `AllowView` |
| Editors | Choosing an editor (`editors`) | Inference, the editor catalog, editor settings |
| | Inputs (`inputs`) | Text, textarea, number, date, checkbox, switch, slider, OTP |
| | Choices (`choice-editors`) | Select, radio, checkbox group, segmented control, toggle group, the empty choice, media, columns |
| | Lists and dictionaries (`list-and-dictionary-editors`) | List and dictionary editors |
| | Images (`image-editor`) | `ImageEditor`, `ResolveUrl`, `StoreImage`, cleanup |
| | Rich text (`rich-text-editor`) | `RichTextEditor`, `HtmlDisplay`, `IRichTextSanitizer` |
| | Icons (`icon-editor`) | `IconEditor`, `IconDisplay` |
| | Custom editors (`custom-editors`) | Overriding templates, `UseEditor<TEditor>` |
| Displays | Built-in displays (`displays`) | How a value renders, the built-in displays |
| | Display templates (`display-templates`) | `DisplayTemplates/` and `GridDisplayTemplates/` |
| | Custom displays (`custom-displays`) | Display classes and handlers |
| Relationships | Foreign keys (`foreign-keys`) | Select vs lookup sheet, lookup settings, create from lookup, related types that aren't resources, lookups without EF Core |
| | Many-to-many (`many-to-many`) | Many-to-many navigations and their editors |
| | Dependent fields (`dependent-fields`) | `DependsOn` and dependent items |
| | Showing related values (`related-values`) | Related records in columns and display fields |
| Panel | Sidebar and links (`sidebar`) | Resource items, custom links, groups, order, icons, items from code |
| | Branding (`branding`) | `ConfigureBranding`, logos |
| | User menu and log out (`user-menu`) | `ConfigureUserMenu`, log out schemes |
| | Home page (`home-page`) | `ConfigureHomePage`, the setup checklist |
| | Environment indicator (`environment-indicator`) | `EnableEnvironmentIndicator` |
| | Host pages (`host-pages`) | Own pages in the admin layout |
| Security | Authorization (`authorization`) | `RequireAuthorization` on the panel, resources and links |
| Customization | Overriding pages (`overriding-pages`) | View imports, overriding and adding to generated pages |
| | Page tag helpers and slots (`page-tag-helpers`) | `sa-index-page` and the other page helpers |
| | Template overrides (`template-overrides`) | Replacing shared partials and templates |
| | Global labels (`global-labels`) | `ConfigureResourceLabels` |
| | Theming (`theming`) | Choosing a theme for the panel; links to the UI theming docs |
| Troubleshooting | Configuration problems (`configuration-problems`) | The Development indicator, logs, `ConfigureDiagnostics` |
| | Installation issues (`installation-troubleshooting`) | Reading a 500, checking without a browser |

51 pages. A page's exact scope may shift while writing when the source shows a better split; any change to the navigation is recorded here.

Changed while writing: "Text and numbers" became "Inputs" (`inputs`), since the page also covers dates, checkboxes, switches, sliders and codes.

## Writing rules

- The source in `src/StellarAdmin` and `src/StellarAdmin.EntityFrameworkCore` is the authority. The consumer skill references are a starting map; where they disagree with the source, the source wins and the disagreement is recorded below (the skill isn't changed in this pass).
- Short and to the point, and sounding like a person wrote it: the audience is developers. Lead with code; a sentence or two per point; no filler (Jerrie, 2026-10-24).
- Consumer-facing prose, following the website's component pages: frontmatter title and description, short usage first, then examples. One line per prose paragraph.
- All sample content uses the Voyager Travel theme (trips, bookings, customers, guides, destinations).
- EF Core is the default in every example; non-EF behaviour is a short subsection or a link to custom data sources.
- Link UI component pages where an editor or display renders one (for example the slider editor to `/docs/ui/components/slider`).

## Verification

- C# examples compile: a scratch web project outside both repositories references the product projects and holds the Voyager entities, and each page's examples are compiled in it.
- Website: `pnpm lint`, `pnpm types:check` and `pnpm build`.
- Every page loads on a dev server I start on a port other than 5205, checked in headless Chromium with internal links resolved; servers I start are stopped afterwards.

## Progress

Scratch check app: `DocsCheck` in the session scratchpad (Voyager model, SQLite seed, `SNIPPET=Class.Method` picks a page's configuration; port 5207). Each section's C# compiles there; behaviour claims are spot-checked against the running app.

- [x] Scaffold: `content/docs/meta.json` lists `admin`; `content/docs/admin/meta.json` has the navigation.
- [x] Getting Started (5 pages), compiled; quick start run.
- [x] Resources (4 pages), compiled; custom data source search run.
- [x] Index Pages (7 pages), compiled; row details (inline, projection, handler, tabs) and inline edit run.
- [x] Forms (5 pages), compiled; field labels, factory and custom model pages run.
- [x] View Pages (1 page), compiled and run.
- [x] Editors (8 pages), compiled; the trip form with every editor, the custom colour editor and its template run.
- [x] Displays (3 pages), compiled; number, boolean, badge and a custom display with its grid template run.
- [x] Relationships (4 pages), compiled; lookups, create from lookup, dependent selects, lookup sources and badges run.
- [x] Panel (6 pages), compiled; sidebar items, links (including the missing-action warning), provider items, branding, environment banner, home page override and host page run. User menu compiled only (the check app has no authentication).
- [x] Security (1 page), Customization (5 pages) and Troubleshooting (2 pages), compiled; index and view overrides with slots, global labels, stylesheets and scripts run. Authorization compiled only.
- [x] Website checks (2026-10-24): `pnpm types:check`, `pnpm lint` and `pnpm build` pass. A link check over the 51 pages finds no missing pages, anchors or navigation entries. On a dev server (port 5310), all 51 pages return 200 with their titles; quick start and choices were screenshotted in headless Chromium and render with the StellarAdmin root in the sidebar. Servers stopped afterwards.

## Source disagreements (fixed 2026-10-25)

The consumer skills disagreed with the source in three places; Jerrie approved fixing them, and they now match the docs:

- A default sort on an unsortable column: `stellar-admin` index-pages and `stellar-admin-setup` said options validation fails every request; `ResourceIndexCheck.CheckDefaultSort` drops the sort and reports `UnsortableDefaultSort`.
- `stellar-admin` inline-editing's first example chained `.EditsField(...)` onto `columns.Add(selector, callback)`, which returns the columns builder and doesn't compile; it now sets it inside the callback.
- `stellar-admin-setup` ("What a working install looks like" and troubleshooting) said options validation errors, including page sizes, fail every request with a 500; it now says configuration mistakes are repaired or hide the resource, and are reported as configuration problems.

## Follow-ups

- Done 2026-10-25: the UI docs' `agent-skills.mdx` points to the admin skills instead of saying there are none.
- Done 2026-10-25: the header's Documentation link is a menu with both docs roots. Its links (like the root switcher's) only render when opened, so `vite.config.ts` now seeds the prerender crawl with `/` and `/docs/admin`.
- Open: demos and screenshots (decision 4).
