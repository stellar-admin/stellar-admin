# StellarAdmin OSS development

Read this guide before working on the product. Shared instructions are in [AGENTS.md](../../AGENTS.md), with conventions in [docs/conventions](../conventions/). Paths below are relative to the product repository root.

Guidance for working in this repository.

## What this is

**StellarAdmin.TagHelpers** is a library of ASP.NET Core **Tag Helpers** that mirror [shadcn/ui](https://ui.shadcn.com/) components, for building MVC / Razor Pages UIs. It ships as the `StellarAdmin.TagHelpers` NuGet package. Consumers register it, add the tag helpers to `_ViewImports.cshtml`, link **one per-theme stylesheet** (`stellar-admin.<theme>.css` — aurora, concourse, ice, ledger, meridian, observatory, parallax, shadcn.luma, shadcn.lyra, shadcn.maia, shadcn.mira, shadcn.nova, shadcn.rhea, shadcn.sera, shadcn.vega), and reference `stellar-admin.js`. **A theme is a CSS file**: switching themes is switching the `<link>`, with no server-side involvement.

Each component is a server-rendered tag helper (`<sa-*>`). Interactivity that can't be done with HTML/CSS alone is provided by small **Lit web components** (`<sel-*>`) bundled into `stellar-admin.js`.

**This repo contains the complete open-source product.** `StellarAdmin.Dashboard`, `StellarAdmin.Dashboard.Identity`, and `StellarAdmin.Dashboard.EntityFrameworkCore` provide the integrated admin application and its integrations; they share the MIT license.

### Registration

`StellarAdmin.Core` owns the shared entry point; TagHelpers depends on it transitively and supplies the UI layer:

```csharp
services.AddStellarAdmin()   // namespace StellarAdmin — returns StellarAdminBuilder
        .AddTagHelpers();            // namespace StellarAdmin.TagHelpers — returns StellarAdminTagHelpersBuilder
```

`AddStellarAdmin(Action<StellarAdminBuilder>)` is also available. `AddStellarAdmin()` registers `IconOptions`, which includes Lucide icons by default. Custom icons and packs are configured through `IconOptions` by `StellarAdminBuilder`; each service provider owns its options instance. Consumers inject `IOptions<IconOptions>` and resolve `.Value` in their constructors. Repeated registration preserves overrides. `AddTagHelpers()` registers the tag helper options. Theme selection is **not** part of registration: it's whichever theme stylesheet the app links.

## Repository layout

| Path | What |
|------|------|
| `src/StellarAdmin.Core/` | Shared DI entry point (`StellarAdminBuilder`, `AddStellarAdmin()`, namespace `StellarAdmin`) and icons (namespace `StellarAdmin.Icons`); depends on DI abstractions and options. |
| `src/StellarAdmin.TagHelpers/` | Tag helpers, theming, client assets, and the `ConfigureForms()` extension and form options. |
| `src/StellarAdmin.TagHelpers/TagHelpers/<Component>/` | One folder per component (e.g. `Sidebar/`, `Button/`, `Sheet/`). |
| `src/StellarAdmin.TagHelpers/Client/` | All client sources: TypeScript and the CSS (`css/theme.css`, `css/theme-tokens.css`, `css/components.css`, `css/anchors.css`, `css/themes/<name>.css` — the generated per-theme `.sa-*` rules), built into `src/StellarAdmin.TagHelpers/wwwroot/`. |
| `src/StellarAdmin.TagHelpers/Client/js/web-components/` | The `sel-*` Lit components. |
| `gen/`, `util/`, gen projects | Source generators (icons) and `util/ThemeGenerator`, the manual-run console app that regenerates `Client/css/themes/*.css` from upstream shadcn styles. |
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
npm run build:css             # Tailwind v4 CLI x15 -> ../wwwroot/stellar-admin.<theme>.css
npm run fmt                   # oxfmt (format TS/JS)
```
`build:css` (`Client/scripts/build-theme-bundles.mjs`) derives the theme list from `Client/css/themes/` and compiles one self-contained bundle per theme, synthesizing each entry (`css/base.css` + the theme file) in a temporary directory — there are no checked-in per-theme entry files. **Nothing scans source files** (`base.css` uses `@import "tailwindcss" source(none)`): the bundle is fully determined by the `.sa-*` rules in `Client/css/components.css` + `css/themes/<theme>.css` and the `@source inline()` safelist at the top of `components.css` (the few utility classes tag helpers still emit as literals). If a tag helper gains a new sanctioned literal, add it to that safelist or it will silently not exist.

**Verify bundle output after CSS changes:** `dotnet build` can report success even when the `Client` target's `build:css` step fails (observed: `Cannot apply unknown utility class` errors left every bundle stale while the build stayed green). After touching `components.css` or the theme files, grep a new class name in `wwwroot/stellar-admin.shadcn.nova.css` — or run `npm run build:css` directly, which does fail loudly — before judging anything in a browser.

### Custom themes and coverage

Theme web fonts are optional: each font stack prefers its selected family and falls back to native fonts. Custom-theme stacks are recorded in their theme specifications; shadcn stacks live in `util/ThemeGenerator/Themes/*.custom.css`. The library never downloads fonts. DocsSamples and website exports share the sample `wwwroot/js/theme-fonts.js` loader, which requests only the selected theme’s families with `display=swap`; preserve native-only operation when extending a theme.

The `shadcn.` namespace is reserved for upstream-derived themes: generator inputs and `Themes/Nova.custom.css` retain upstream names, while generated sources use `Client/css/themes/shadcn.nova.css` and ship as `stellar-admin.shadcn.nova.css`. Custom themes keep unprefixed names. Coverage validation enforces ownership of the namespace.

Aurora, Ice, Concourse, Ledger, Meridian and Observatory are hand-authored in `Client/css/themes/aurora.css`, `Client/css/themes/ice.css`, `Client/css/themes/concourse.css`, `Client/css/themes/ledger.css`, `Client/css/themes/meridian.css`, and `Client/css/themes/observatory.css`; their maintained specifications are [Aurora](../design/themes/aurora.md), [Ice](../design/themes/ice.md), [Concourse](../design/themes/concourse.md) [Ledger](../design/themes/ledger.md), [Meridian](../design/themes/meridian.md) and [Observatory](../design/themes/observatory.md). They supply their own palettes and font families. Apps may optionally load Source Sans 3 and IBM Plex Mono for Concourse, or Lexend and JetBrains Mono for Ledger. The eight other theme files remain generated from shadcn. Their maintained `Themes/*.custom.css` inputs add optional font defaults: Inter for Luma/Mira/Rhea/Vega, Geist for Nova, Figtree for Maia, JetBrains Mono for Lyra, and Noto Sans with Playfair Display titles for Sera. Sera uses `--sa-sera-font-display` and the existing `sa-font-heading` hook; `--font-heading` remains a consumer escape hatch. The CSS build runs `util/theme-coverage/check.mjs` before compiling. Update its explicit component/theme manifest after reviewing support for any new component, subcomponent, or styling hook. Shared-only entries require a rationale; pending support fails the build. See `util/theme-coverage/README.md`.

Ice optionally loads IBM Plex Sans (400/500/600) and JetBrains Mono (400/500/700) from the app layout.

DocsSamples accepts `?theme=ice&mode=dark`, `?theme=concourse&mode=dark` and `?theme=ledger&mode=dark`. Use those query strings on ordinary component pages to review the selected theme; there are no dedicated Ledger sample pages. VRT accepts `--theme ledger --mode light|dark`.

Observatory uses IBM Plex Sans (400/500/600) and IBM Plex Mono (400/500), 32px default controls and compact table spacing. Its maintained specification is [Observatory](../design/themes/observatory.md). DocsSamples accepts `?theme=observatory&mode=light|dark`.

### Regenerating the theme CSS (manual, on shadcn updates)
```bash
dotnet run --project util/ThemeGenerator   # downloads shadcn style-*.css -> Client/css/themes/shadcn.*.css
```
Adding a theme: author or generate its `Client/css/themes/<name>.css` and add a `ClientOutput` line for its bundle inside the `ClientItems` target in `StellarAdmin.TagHelpers.csproj`. The `ClientOutput` items are declared in that target rather than a top-level `ItemGroup` on purpose: Rider silently strips evaluation-time items whose file is deleted in the IDE (that's how they once went missing, leaving the `Client` target's `Outputs` empty — and MSBuild *skips* a target with inputs but no outputs, so client builds silently stopped running). Execution-time items are invisible to Rider but fully visible to the `Client` target's up-to-date check.

`dotnet build` runs `npm run build` for you via the `Client` target, which skips when none of its inputs changed. It hooks `ResolveProjectStaticWebAssets` rather than `Build`: `wwwroot/` is gitignored, so on a clean checkout a `BeforeTargets="Build"` target would run *after* static web asset discovery had already found the folder empty — leaving the first build with no `_content/` assets.

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
- Classes are composed with `JoinCssClasses("sa-...", output.GetUserSuppliedClass())` (a static on the base class) — a plain null-skipping string join, nothing more. **Styling belongs in `Client/css/components.css` (structural) or the theme files, not in C# literals.** The only literals allowed in a merge are the sanctioned set: marker classes (`group/x`, `peer/x`, `dark`), `size-4` on icons (theme rules sniff `[class*='size-']`), `sr-only`, Field's child-width forcing, and `font-heading` — each must also be in the `@source inline()` safelist in `components.css`. Conflict resolution is the cascade's job (author utilities out-rank component rules by layer order). `font-heading` is **literal-only**: no theme defines `--font-heading` in-library (it's a consumer escape hatch), so `@apply font-heading` inside `components.css` fails every theme-bundle compile with "Cannot apply unknown utility class" — title-ish tag helpers emit it from C# instead, alongside the `sa-font-heading` hook class (see `DialogTitleTagHelper`, `SheetTitleTagHelper`, `PageHeaderTitleTagHelper`).
- `output.GetUserSuppliedClass()` (extension in `TagHelpers/TagHelperOutputExtensions.cs`) reads the author's `class` (read-only; leaves it on `output`).
- Emit a `data-slot="..."` on the primary element (shadcn convention; also used as a styling/query hook).

### Theming & component styling
- **A theme is a stylesheet.** Themed declarations live as `.sa-*` rules in `Client/css/themes/<theme>.css` (generated by `util/ThemeGenerator` from upstream shadcn styles, wrapped in the nested `@layer components.theme`). The theme-independent structural half lives in the hand-maintained `Client/css/components.css` under the same class names, directly in `@layer components`. Nothing resolves classes server-side.
- **Precedence, lowest to highest:** theme rules (`components.theme` sublayer) < structural rules (directly in `components` — styles outside a nested layer beat the nested layer) < utilities. So structure wins same-property conflicts with a theme, and an author's `class` utilities beat everything.
- A component class name passed to `JoinCssClasses` is emitted verbatim on the element. A name with no rule renders as a harmless dead class.
- State/variant styling keys off the `data-*` attributes tag helpers emit (`[data-side]`, `[data-orientation]`, `[data-anchor-side]`, …) as attribute selectors inside the component rules — never off extra marker classes.
- One marker-class exception: `MenuColor.Inverted` emits the literal `dark` class (re-scopes theme variables on the element), which a stylesheet rule cannot express.
- **Utilities that override *other elements'* classes must stay in the class attribute** (utilities layer), e.g. Field's `[&>*]:w-full` — a components-layer rule cannot reliably beat a child's own component classes.

### Established C# patterns (follow these — they're enforced in review)

- **Formatting:** follow [method spacing and braces](../conventions/csharp-file-organization.md#method-formatting) and [multiline XML comments](../conventions/xml-documentation.md#formatting). Always brace `if`/`else`; separate logical sections in `ProcessAsync`. A formatter pass alone does not enforce these choices.
- **Enum → data-attribute text** lives in an **extension method**, not inline `switch`/`if`. Add a `GetDataAttributeText()` in a C# 14 `extension(...)` block next to the enum. Reference: `TagHelpers/Separator/SeparatorOrientation.cs`, `TagHelpers/Sidebar/SidebarSide.cs`.
- **No default values on bound enum/bool properties.** Make them nullable and resolve at the top of `ProcessAsync`: `var effectiveSide = Side ?? SidebarSide.Left;`. Reference: `SeparatorTagHelper`, `SidebarTagHelper`.
- **Inline single-use locals** rather than naming a value used exactly once.
- **Don't fetch child content just to re-append it.** A tag helper that never processes child content gets it rendered inside the tag automatically — `output.Content.AppendHtml(await output.GetChildContentAsync())` is redundant, and without it `ProcessAsync` can be synchronous (`return Task.CompletedTask;`). Only call `GetChildContentAsync()` when the content must actually be relocated or wrapped (e.g. `TabListTagHelper` nesting it in an inner `TagBuilder`). Reference: `PageContainerTagHelper`.
- **Rendering a button:** prefer calling `ButtonRenderingHelper.RenderAttributes(output, variant, size)` directly on the element you're rendering, rather than instantiating a `ButtonTagHelper` and suppressing a wrapper. Reference: `InputGroupButtonTagHelper`, `PaginationLinkTagHelper`, `SidebarTriggerTagHelper`. (`RenderAttributes` only sets `data-slot="button"` if none is already present, and folds in the user class.)

### CSS consumption model
The prebuilt per-theme bundle is the **only** consumption mode: a consumer links exactly one `_content/StellarAdmin.TagHelpers/stellar-admin.<theme>.css`. There is no "single-build" mode where an app's own Tailwind build compiles the library's styles, and nothing CSS-related is shipped in the nupkg besides the bundles — no packed sources, no `.targets`.

- Each bundle is compiled from `Client/css/base.css` (`@import "tailwindcss" source(none)`, `tw-animate-css` from npm, then theme.css / shadcn-tailwind.css / anchors.css / components.css) plus one `Client/css/themes/<theme>.css`.
- **Theming customization is plain CSS**: an app redeclares the custom properties (`:root { --primary: …; --radius: … }`) in its own stylesheet — no imports needed; every compiled declaration references `var(--…)`.
- An app whose *own markup* uses token-named utilities (`bg-background`, …) adds `Client/css/theme-tokens.css` (the `@theme` vocabulary + `dark:` variant, no values) to its own Tailwind build so those utilities can be generated. External consumers copy the file (or the token block from the docs) into their project; the sample apps import it repo-relatively (see `docs/DocsSamples/Client/css/site.css`).

### Cascade layers
Tailwind's stock layer order applies (`theme, base, components, utilities`); layer names unify across same-document stylesheets, so the app's utilities out-rank the library's component rules no matter the `<link>` order. Within `components`, the generated theme rules sit in the nested `@layer components.theme`, which loses to the structural rules declared directly in `components` (see Theming above). The library emits so few utilities (the `@source inline()` safelist) that no special layer machinery is needed anymore — the old `stellar-admin` promotion layer is gone.

Optional webfonts are linked from the app layout, **not** `@import`ed in CSS. A remote `@import` inside a nested imported file gets emitted after the `@layer` blocks, and browsers drop it per spec — silently, with no console error.

### Client web components (`sel-*`)

- **TypeScript formatting:** separate logical groups of statements with blank lines inside methods. Keep related statements together, and separate completed conditional blocks from the next independent operation. Do not insert a blank line between every statement. Follow `sel-carousel.ts` as a reference; running oxfmt alone does not enforce this spacing.
- Built with **Lit**, but rendered in **light DOM** (`createRenderRoot() { return this; }`) so server-rendered children stay styleable by Tailwind and participate in layout. Don't use shadow DOM here.
- Register a new component by adding `import "./web-components/sel-foo";` to `Client/js/stellar-admin-ui.ts`.
- **Activation uses the native Invoker Commands API**, not click handlers. A button carries `command="--custom"` + `commandfor="<id>"`; the browser dispatches a `command` event on the element with that id, and the component handles it (`addEventListener("command", ...)`, switching on `event.command`). The `interestfor` polyfill is bundled. Reference: `sel-collapsible.ts`, `sel-sidebar.ts`, and on the server side `SheetTagHelper` (auto-generates an id with `context.UniqueId` when the author didn't supply one). **Do not** invent `data-*` "marker" attributes with delegated click handlers — that pattern was tried and removed for being inconsistent with the rest of the repo.
- State is exposed to CSS by reflecting it onto `data-*` attributes that Tailwind `group-data-[...]` variants react to.

## Verifying changes
Follow the [unit testing conventions](../conventions/unit-testing.md) for new and migrated .NET tests: mirror the owning SUT's project and folders, use TUnit, and write explicit arrange–act–assert sections. `StellarAdmin.Core.Tests` runs icon configuration and DI registration tests through TUnit. `StellarAdmin.TagHelpers.Tests` uses TUnit for component rendering and TagHelpers registration tests. The EntityFrameworkCore executable retains existing checks awaiting migration; keep running it for relevant changes. Also verify component work by running the DocsSamples site (`docs/DocsSamples`) and exercising the relevant `Pages/<Component>/` sample in the browser (desktop + mobile widths where applicable).

DocsSamples consume the prebuilt bundles, with deliberate variation — DocsSamples links the Observatory theme, ComponentPlayground links shadcn.vega and additionally runs the `@tailwindcss/forms` plugin in its own build. Both import `theme-tokens.css` into their own Tailwind builds, keeping the token-vocabulary consumer path exercised.

### Visual-regression tool
`util/visual-regression/vrt.mjs` (Node + system chromium over CDP; pixelmatch/pngjs via its own `package.json`) screenshots every DocsSamples page at two viewports plus overlay open-state scenarios, then pixel-diffs two capture runs (anti-aliasing-classified pixels are counted separately as hairline changes — same-environment captures are byte-identical, so they are signal, not noise). Pixel comparison is only deterministic within one environment: capture base and head on the same machine in one sitting — never compare captures from different machines or browser builds. Captures are on-demand and gitignored (`util/visual-regression/snapshots/`).
```bash
node util/visual-regression/vrt.mjs capture --url http://localhost:5205 --out util/visual-regression/snapshots/<name>   # DocsSamples must be running
node util/visual-regression/vrt.mjs compare util/visual-regression/snapshots/<base> util/visual-regression/snapshots/<head>   # report.md, summary.json + diff images
```
PRs run this automatically (`.github/workflows/vrt.yml`): an ephemeral base-vs-head comparison on one runner, reported as an advisory sticky PR comment (`vrt-comment.yml`) with the screenshot pairs in the run artifact — contributors never regenerate baselines. `selftest.mjs` proves the pipeline fires on known historical regressions (standing rule: any visual bug that reaches human eyes without the tool firing becomes a permanent self-test case) and asserts a clean re-capture produces zero diffs.

Meridian is hand-authored in `Client/css/themes/meridian.css`, with its [maintained specification](../design/themes/meridian.md). Optionally load Instrument Sans 600, Work Sans 400/500/600 and JetBrains Mono 400/500 from the application layout. DocsSamples accepts `?theme=meridian&mode=light|dark`. Verify it with `node util/visual-regression/verify-meridian.mjs http://localhost:5206`; optional `MERIDIAN_FONT_CSS` supplies embedded font faces; grid checks use the same sample app.

Aurora is hand-authored in `Client/css/themes/aurora.css`; see its [specification](../design/themes/aurora.md). Optionally load Archivo 400/500/600 and IBM Plex Mono 400/500 from the layout. DocsSamples accepts `?theme=aurora&mode=light|dark`. Verify with `node util/visual-regression/verify-aurora.mjs http://localhost:5206`; optional `AURORA_FONT_CSS` supplies local font CSS; grid checks use the same sample app.

Parallax is hand-authored in `Client/css/themes/parallax.css`; see its [specification](../design/themes/parallax.md). Optionally load Space Grotesk 400/500/600 and JetBrains Mono 400/500 in the layout. DocsSamples accepts `?theme=parallax&mode=light|dark`. Verify with `node util/visual-regression/verify-parallax.mjs http://localhost:5206`; optional `PARALLAX_FONT_CSS` supplies local fonts; grid checks use the same sample app. Keep it outside ThemeGenerator.

## Resources, Identity, and Entity Framework Core

The admin shell and resource layer live in `src/StellarAdmin.Dashboard/`, with Identity and EF Core integrations in their respective `src/StellarAdmin.Dashboard.*` projects. Register the application with `AddStellarAdmin().AddDashboard()`; its assets use `_content/StellarAdmin.Dashboard/`. Maintained Identity designs live in `docs/design/identity-configuration.md` and `docs/design/identity-user-forms.md`.

`docs/DocsSamples/` includes DataGrid. `docs/DocsSamplesGenerator/` exports website demos. `sandbox/IdentitySimplePlayground/` is also the host for the EF Core integration suite; keep it available when running tests.

```bash
dotnet build StellarAdmin.slnx
dotnet run --project tests/StellarAdmin.Core.Tests --configuration Release
dotnet run --project tests/StellarAdmin.TagHelpers.Tests --configuration Release
dotnet run --project tests/StellarAdmin.Dashboard.EntityFrameworkCore.Tests
```
