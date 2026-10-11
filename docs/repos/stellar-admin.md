# StellarAdmin OSS development

Read this guide before working on the product. Shared instructions are in [AGENTS.md](../../AGENTS.md), with conventions in [docs/conventions](../conventions/). Paths below are relative to the product repository root.

Guidance for working in this repository.

## What this is

**StellarAdmin.TagHelpers** is a library of ASP.NET Core **Tag Helpers** that mirror [shadcn/ui](https://ui.shadcn.com/) components, for building MVC / Razor Pages UIs. It ships as the `StellarAdmin.TagHelpers` NuGet package. Consumers register it, add the tag helpers to `_ViewImports.cshtml`, link `stellar-admin.css` (optionally followed by their own knob file, for example one downloaded from the [theme builder](https://www.stellaradmin.com/theme-builder)), and reference `stellar-admin.js`. **A theme is a stylesheet of knob values**: switching themes is switching the `<link>`, with no server-side involvement.

Each component is a server-rendered tag helper (`<sa-*>`). Interactivity that can't be done with HTML/CSS alone is provided by small **Lit web components** (`<sel-*>`) bundled into `stellar-admin.js`.

**This repo contains the complete open-source product.** `StellarAdmin.Dashboard` and `StellarAdmin.Dashboard.EntityFrameworkCore` provide the integrated admin application and EF Core resources; they share the MIT license. `sandbox/DashboardPlayground` demonstrates Identity management through ordinary resources.

### Registration

`StellarAdmin.Core` owns the shared entry point; TagHelpers depends on it transitively and supplies the UI layer:

```csharp
services.AddStellarAdmin()   // namespace StellarAdmin — returns StellarAdminBuilder
        .AddTagHelpers();            // namespace StellarAdmin.TagHelpers — returns StellarAdminTagHelpersBuilder
```

`AddStellarAdmin(Action<StellarAdminBuilder>)` is also available. `AddStellarAdmin()` registers `IconOptions`, which includes Lucide icons by default. Custom icons and packs are configured through `IconOptions` by `StellarAdminBuilder`; each service provider owns its options instance. Consumers inject `IOptions<IconOptions>` and resolve `.Value` in their constructors. Repeated registration preserves overrides. `AddTagHelpers()` registers the tag helper options. TagHelpers-only apps select a theme by linking their own knob file; Dashboard apps add it with `dashboard.AddStylesheet(...)`.

## Repository layout

| Path | What |
|------|------|
| `src/StellarAdmin.Core/` | Shared DI entry point (`StellarAdminBuilder`, `AddStellarAdmin()`, namespace `StellarAdmin`) and icons (namespace `StellarAdmin.Icons`); depends on DI abstractions and options. |
| `src/StellarAdmin.TagHelpers/` | Tag helpers, theming, client assets, and the `ConfigureForms()` extension and form options. |
| `src/StellarAdmin.TagHelpers/TagHelpers/<Component>/` | One folder per component (e.g. `Sidebar/`, `Button/`, `Sheet/`). |
| `src/StellarAdmin.TagHelpers/Client/` | All client sources: TypeScript and the plain CSS (`css/reset.css`, `css/tokens.css`, `css/structure.css`, `css/components/<component>.css`, `css/tailwind-adapter.css`) and the knob manifest (`css/knobs.json`), built into `src/StellarAdmin.TagHelpers/wwwroot/`. |
| `src/StellarAdmin.TagHelpers/Client/js/web-components/` | The `sel-*` Lit components. |
| `gen/`, `util/`, gen projects | Source generators (icons), the theme checks (`util/theme-check/`: contrast and screenshots across theme fixtures and random themes), visual regression (`util/visual-regression/`) and the skills generator. |
| `sandbox/` | Throwaway prototypes (e.g. `sandbox/html/*.html` for validating CSS approaches). For a new visual design, use the product `prototype-component` skill; for an existing upstream design, use `port-shadcn-component`. The development skills live in `.agents/skills/` (product-root-relative). Prototypes are point-in-time artifacts, code flows prototype → library only. |

Solution file: `StellarAdmin.slnx`. SDK pinned in `global.json` (`.NET 10`). Packages are centrally managed in `Directory.Packages.props`.

## Build & dev commands

### .NET
```bash
dotnet build src/StellarAdmin.TagHelpers/StellarAdmin.TagHelpers.csproj
```

### Client assets (run from `src/StellarAdmin.TagHelpers/Client/`)
```bash
npm run build                 # build:js + build:css
npm run build:js              # rolldown -> ../wwwroot/stellar-admin.js  (IIFE, minified)
npm run build:css             # plain CSS -> ../wwwroot/stellar-admin.css, stellar-admin.tailwind.css, stellar-admin.knobs.json
npm run fmt                   # oxfmt (format TS/JS)
```
`build:css` (`Client/scripts/build-css.mjs`) runs no CSS compiler: it concatenates the layer order, `reset.css`, `tokens.css`, `structure.css` and every `components/*.css` file in name order into `stellar-admin.css`, and copies `tailwind-adapter.css` as it is. It also writes `stellar-admin.knobs.json` from `css/knobs.json`, the knob manifest the website's theme builder reads (label, group, meaning, type, range or choices, default; optional knobs name the knob they follow), and fails when the manifest and the stylesheets disagree: a knob in `tokens.css` without an entry, an entry that is no knob, a different default, or an optional knob no stylesheet reads through a `var()` fallback. Adding or changing a knob means updating `knobs.json` too. Native nesting and the colour functions stay as written, which sets the [browser baseline](../../readme.md#browser-support). Knobs, foundation tokens and the theme grammar are documented in [tokens](../design/tokens.md).

The library ships no presets: themes come from the [theme builder](https://www.stellaradmin.com/theme-builder), which seeds from its own copies of the former presets. Adding a client output: add a `ClientOutput` line for it inside the `ClientItems` target in `StellarAdmin.TagHelpers.csproj`. The `ClientOutput` items are declared in that target rather than a top-level `ItemGroup` on purpose: Rider silently strips evaluation-time items whose file is deleted in the IDE (that's how they once went missing, leaving the `Client` target's `Outputs` empty — and MSBuild *skips* a target with inputs but no outputs, so client builds silently stopped running). Execution-time items are invisible to Rider but fully visible to the `Client` target's up-to-date check.

`dotnet build` runs `npm run build` for you via the `Client` target, which skips when none of its inputs changed. It hooks `ResolveProjectStaticWebAssets` rather than `Build`: `wwwroot/` is gitignored, so on a clean checkout a `BeforeTargets="Build"` target would run *after* static web asset discovery had already found the folder empty — leaving the first build with no `_content/` assets.

Theme web fonts are optional: each font stack prefers its family and falls back to native fonts, and the library never downloads fonts (a builder theme file may `@import` its Google Fonts). DocsSamples and website exports share the sample `wwwroot/js/theme-fonts.js` loader, which requests only the selected theme's family with `display=swap`.

### Formatting
- **C#:** CSharpier (local dotnet tool, `.config/dotnet-tools.json`). Restore tools with `dotnet tool restore`, then run `dotnet csharpier format <touched-path>` from this repo (or rely on format-on-save). Match the existing formatting in edits.
- **TS/JS:** `oxfmt` via `npm run fmt`.

## Architecture & conventions

### Tag helpers
- Inherit `StellarAdminTagHelperBase` (or `StellarAdminAnchorTagHelperBase` for anchors). Most tag helpers need no constructor at all; inject extras (e.g. `IOptions<IconOptions>`) when needed.
- A tag helper whose markup should be overridable by the consuming app derives from `StellarAdminTemplatedTagHelperBase` instead: it names a view (`ViewName`, swappable per instance via the `view` attribute) that resolves like a partial view — so apps override by shadowing the view — rendered with `GetViewModel()`'s result as its model (abstract — every subclass states its model explicitly). Child content is executed so `<sa-slot-content>` children register their slots (the base passes itself to the view through its `ViewDataDictionary`), but any other child output is discarded; the view renders a slot with `<sa-slot-outlet name="...">`, whose own children are the fallback when the slot is unfilled or the view renders as a plain partial. Demos: DocsSamples `Pages/TemplatedTagHelper/` and `Pages/SlotOutlet/`.
- **Pass shared configuration through a typed context.** A compound Tag Helper publishes an internal sealed `<Component>Context` with `SetContext(context, value)` before children render; children read it with `GetContext<T>(context)`. Store generated IDs and resolved option values in required init-only properties, so defaults are resolved once by the parent. Do not add internal properties to the parent Tag Helper merely for children to retrieve with `GetParentTagHelper`. References: `CarouselContext`, `CarouselTagHelper`, and `DropdownMenuContext`. Razor scopes `TagHelperContext.Items` to descendants, so a nested component can publish its own context without overwriting its outer sibling's configuration.
- `GetParentTagHelper<TParent>()` remains available when a child needs the ancestor instance itself. Existing uses such as `SidebarTriggerTagHelper` are historical patterns, not the template for passing IDs/configuration in new components. Do not refactor unrelated components as a drive-by.
- Any StellarAdmin tag helper can host **named slots**: a caller puts `<sa-slot-content name="x">` among the children; it assigns its content to the nearest StellarAdmin ancestor (`TryAddNamedSlot`) and suppresses itself. The host executes its child content (`GetChildContentAsync` — legitimate relocation) and reads slots back with `TryGetNamedSlot` to render them wherever it chooses; a filled slot the host never reads renders nothing. Demo: DocsSamples `TagHelpers/BookingSummaryTagHelper.cs` + `Pages/SlotContent/`.
- Classes are composed with `JoinCssClasses("sa-...", output.GetUserSuppliedClass())` (a static on the base class) — a plain null-skipping string join, nothing more. **Styling belongs in `Client/css/components/<component>.css`, not in C# literals.** Tag helpers emit component classes and `data-*` attributes only, plus the few structure classes in `structure.css`: `sr-only`, `size-4` on icons (component rules size an icon only when its class names no size, `svg:not([class*="size-"])`) and the `anchored-*` position classes. Conflict resolution is the cascade's job (author classes out-rank component rules by layer order).
- `output.GetUserSuppliedClass()` (extension in `TagHelpers/TagHelperOutputExtensions.cs`) reads the author's `class` (read-only; leaves it on `output`).
- Emit a `data-slot="..."` on the primary element (shadcn convention; also used as a styling/query hook).

### Theming & component styling
- **A theme is knob values.** `tokens.css` holds the knobs (tier 0) and the foundation tokens derived from them (tier 1); components read only those and their own private `--_*` variables. An app's theme file sets knobs in `@layer sa.theme`; raw `.sa-*` rules in `@layer sa.overrides` are the escape hatch. See [tokens](../design/tokens.md).
- Each component's rules live in `Client/css/components/<component>.css`, in `@layer sa.components` (private variables that differ by mode in `@layer sa.tokens`). A file must not rely on another file's order: a rule that adjusts another component's part out-ranks it by selector.
- A component class name passed to `JoinCssClasses` is emitted verbatim on the element. A name with no rule renders as a harmless dead class.
- State/variant styling keys off the `data-*` attributes tag helpers emit (`[data-side]`, `[data-orientation]`, `[data-anchor-side]`, …) and native state (`:checked`, `:has()`), never off marker classes.
- One marker-class exception: `MenuColor.Inverted` emits the literal `dark` class (re-scopes the tokens on the element), which a stylesheet rule cannot express.

### Established C# patterns (follow these — they're enforced in review)

- **Formatting:** follow [method spacing and braces](../conventions/csharp-file-organization.md#method-formatting) and [multiline XML comments](../conventions/xml-documentation.md#formatting). Always brace `if`/`else`; separate logical sections in `ProcessAsync`. A formatter pass alone does not enforce these choices.
- **Enum → data-attribute text** lives in an **extension method**, not inline `switch`/`if`. Add a `GetDataAttributeText()` in a C# 14 `extension(...)` block next to the enum. Reference: `TagHelpers/Separator/SeparatorOrientation.cs`, `TagHelpers/Sidebar/SidebarSide.cs`.
- **No default values on bound enum/bool properties.** Make them nullable and resolve at the top of `ProcessAsync`: `var effectiveSide = Side ?? SidebarSide.Left;`. Reference: `SeparatorTagHelper`, `SidebarTagHelper`.
- **Inline single-use locals** rather than naming a value used exactly once.
- **Don't fetch child content just to re-append it.** A tag helper that never processes child content gets it rendered inside the tag automatically — `output.Content.AppendHtml(await output.GetChildContentAsync())` is redundant, and without it `ProcessAsync` can be synchronous (`return Task.CompletedTask;`). Only call `GetChildContentAsync()` when the content must actually be relocated or wrapped (e.g. `TabListTagHelper` nesting it in an inner `TagBuilder`). Reference: `PageContainerTagHelper`.
- **Rendering a button:** prefer calling `ButtonRenderingHelper.RenderAttributes(output, variant, size)` directly on the element you're rendering, rather than instantiating a `ButtonTagHelper` and suppressing a wrapper. Reference: `InputGroupButtonTagHelper`, `PaginationLinkTagHelper`, `SidebarTriggerTagHelper`. (`RenderAttributes` only sets `data-slot="button"` if none is already present, and folds in the user class.)

### CSS consumption model
A consumer links `_content/StellarAdmin.TagHelpers/stellar-admin.css`, then optionally its own knob file. Nothing CSS-related is shipped in the nupkg besides that stylesheet, the Tailwind adapter and the knob manifest — no packed sources, no presets, no `.targets`.

- **Theming customization is plain CSS**: an app sets knobs in its own stylesheet (`@layer sa.theme { :root { --sa-accent: …; --sa-radius: … } }`); every component reads them through the tokens.
- An app whose *own markup* uses Tailwind utilities imports `stellar-admin.tailwind.css` (source: `Client/css/tailwind-adapter.css`) after Tailwind in its own build. It maps spacing, small text, fonts, radii and the shadcn colour names (`bg-primary`, `text-muted-foreground`, …) onto the tokens, inline, so those utilities follow density and dark mode. The sample apps and the Dashboard import it repo-relatively (see `docs/DocsSamples/Client/css/site.css`).

### Cascade layers
`stellar-admin.css` declares `theme, base, sa.reset, sa.tokens, sa.components, components, sa.theme, sa.overrides, utilities`. Tailwind's layer names are included so that, in an app that also uses Tailwind, preflight sits below the components and utilities above them, so an author's classes win. The first stylesheet to declare layers fixes the order, so an app's Tailwind entry declares the same order before `@import "tailwindcss"` (the adapter's header shows it).

Optional webfonts are linked from the app layout, **not** `@import`ed in CSS. A remote `@import` inside a nested imported file gets emitted after the `@layer` blocks, and browsers drop it per spec — silently, with no console error.

### Client web components (`sel-*`)

- **TypeScript formatting:** separate logical groups of statements with blank lines inside methods. Keep related statements together, and separate completed conditional blocks from the next independent operation. Do not insert a blank line between every statement. Follow `sel-carousel.ts` as a reference; running oxfmt alone does not enforce this spacing.
- Built with **Lit**, but rendered in **light DOM** (`createRenderRoot() { return this; }`) so server-rendered children stay styleable by the page's CSS and participate in layout. Don't use shadow DOM here.
- Register a new component by adding `import "./web-components/sel-foo";` to `Client/js/stellar-admin-ui.ts`.
- **Activation uses the native Invoker Commands API**, not click handlers. A button carries `command="--custom"` + `commandfor="<id>"`; the browser dispatches a `command` event on the element with that id, and the component handles it (`addEventListener("command", ...)`, switching on `event.command`). The `interestfor` polyfill is bundled. Reference: `sel-collapsible.ts`, `sel-sidebar.ts`, and on the server side `SheetTagHelper` (auto-generates an id with `context.UniqueId` when the author didn't supply one). **Do not** invent `data-*` "marker" attributes with delegated click handlers — that pattern was tried and removed for being inconsistent with the rest of the repo.
- State is exposed to CSS by reflecting it onto `data-*` attributes that the component rules select on.

## Verifying changes
Follow the [unit testing conventions](../conventions/unit-testing.md) for new and migrated .NET tests: mirror the owning SUT's project and folders, use TUnit, and write explicit arrange–act–assert sections. `StellarAdmin.Core.Tests` runs icon configuration and DI registration tests through TUnit. `StellarAdmin.TagHelpers.Tests` uses TUnit for component rendering and TagHelpers registration tests. Dashboard builder tests live in `StellarAdmin.Dashboard.Tests`; Dashboard rendering/binding and EF resource behavior live in their respective `.IntegrationTests` projects. Both CI and release discover all five TUnit test projects in `StellarAdmin.slnx` automatically through `dotnet test`; adding a test project to the solution needs no workflow entry. There are no legacy test runners. Also verify component work by running the DocsSamples site (`docs/DocsSamples`) and exercising the relevant `Pages/<Component>/` sample in the browser (desktop + mobile widths where applicable).

DocsSamples and ComponentPlayground link `stellar-admin.css` and have a theme picker (`?theme=ledger|ops|soft&mode=light|dark`) over the theme fixtures in `util/theme-check/presets/` (the former presets), which both projects serve from `/presets/` as linked content; the docs export takes them from there for the website's docs theme picker. ComponentPlayground additionally runs the `@tailwindcss/forms` plugin in its own build. Both import the Tailwind adapter into their own Tailwind builds, keeping that consumer path exercised. `util/theme-check/` checks text contrast and takes screenshots across the theme fixtures and seeded random themes (see its README).

### Visual-regression tool
`util/visual-regression/vrt.mjs` (Node + system chromium over CDP; pixelmatch/pngjs via its own `package.json`) screenshots every DocsSamples page at two viewports plus overlay open-state scenarios, then pixel-diffs two capture runs (anti-aliasing-classified pixels are counted separately as hairline changes — same-environment captures are byte-identical, so they are signal, not noise). Pixel comparison is only deterministic within one environment: capture base and head on the same machine in one sitting — never compare captures from different machines or browser builds. Captures are on-demand and gitignored (`util/visual-regression/snapshots/`).
```bash
node util/visual-regression/vrt.mjs capture --url http://localhost:5205 --out util/visual-regression/snapshots/<name>   # DocsSamples must be running
node util/visual-regression/vrt.mjs compare util/visual-regression/snapshots/<base> util/visual-regression/snapshots/<head>   # report.md, summary.json + diff images
```
PRs run this automatically (`.github/workflows/vrt.yml`): an ephemeral base-vs-head comparison on one runner, reported as an advisory sticky PR comment (`vrt-comment.yml`) with the screenshot pairs in the run artifact — contributors never regenerate baselines. `selftest.mjs` proves the pipeline fires on known historical regressions (standing rule: any visual bug that reaches human eyes without the tool firing becomes a permanent self-test case) and asserts a clean re-capture produces zero diffs.

## Resources, Identity, and Entity Framework Core

The admin shell and resource layer live in `src/StellarAdmin.Dashboard/`, with the EF Core integration in `src/StellarAdmin.Dashboard.EntityFrameworkCore/`. Register the application with `AddStellarAdmin().AddDashboard()`; its assets use `_content/StellarAdmin.Dashboard/`. Dashboard links `stellar-admin.css` and then each stylesheet registered with `dashboard.AddStylesheet(...)`, in registration order; a knob stylesheet (`~/css/theme.css`) and a web font link are both added that way. The Dashboard's own Tailwind build (`src/StellarAdmin.Dashboard/Client/css/client.css`) imports the adapter, so its utilities follow the tokens. `sandbox/DashboardPlayground/` demonstrates host-owned Identity user and role resources. `dashboard.RequireAuthorization(...)` and `resource.RequireAuthorization(...)` mirror the framework's endpoint overloads. `MapStellarAdmin` applies them as endpoint metadata, returns the route's convention builder, and the resource sidebar provider hides links the current user cannot open. Resource routes use lowercase kebab-case plural slugs by default, overridable through `AddResource<TResource>(slug)`, and the Dashboard route lowercases generated controller and action segments without affecting host routes; see `docs/development.md`. The playground requires a signed-in user and restricts users and roles to administrators. The former dedicated Identity package designs in `docs/design/identity-configuration.md` and `docs/design/identity-user-forms.md` are retained as superseded history.

`docs/DocsSamples/` includes DataGrid. `docs/DocsSamplesGenerator/` exports website demos. Dashboard HTTP integration tests use an in-process host and private in-memory source. EF Core HTTP integration tests use an in-process host with private in-memory SQLite databases; neither suite needs DashboardPlayground or its database. See [development and verification](../development.md).

```bash
dotnet build StellarAdmin.slnx
dotnet test --solution StellarAdmin.slnx --no-build --configuration Release --minimum-expected-tests 1
```
