# CLAUDE.md

Guidance for working in this repository.

## What this is

**StellarAdmin.UI** is a library of ASP.NET Core **Tag Helpers** that mirror [shadcn/ui](https://ui.shadcn.com/) components, for building MVC / Razor Pages UIs. It ships as the `StellarAdmin.UI` NuGet package. Consumers register it, add the tag helpers to `_ViewImports.cshtml`, link **one per-theme stylesheet** (`stellar-admin-ui.<theme>.css` — vega, nova, luma, lyra, maia, mira, rhea, sera), and reference `stellar-admin-ui.js`. **A theme is a CSS file**: switching themes is switching the `<link>`, with no server-side involvement.

Each component is a server-rendered tag helper (`<sa-*>`). Interactivity that can't be done with HTML/CSS alone is provided by small **Lit web components** (`<sel-*>`) bundled into `stellar-admin-ui.js`.

**This repo is the free, open-source layer.** Closed-source paid extensions build on top of it in separate repos, so keep additions here to what belongs in the OSS package.

### Registration

`StellarAdmin.Core` owns the shared entry point; `StellarAdmin.UI` layers onto it:

```csharp
services.AddStellarAdmin()   // StellarAdmin.Core — returns StellarAdminBuilder
        .AddUI();            // StellarAdmin.UI  — returns StellarAdminUIBuilder
```

`AddStellarAdmin(Action<StellarAdminBuilder>)` is also available. `AddUI()` is what registers `IIconManager` and applies the Lucide icon pack default — `AddStellarAdmin()` on its own does not pull in the UI services. Theme selection is **not** part of registration: it's whichever theme stylesheet the app links.

## Repository layout

| Path | What |
|------|------|
| `src/StellarAdmin.Core/` | Shared DI entry point — `StellarAdminBuilder`, `AddStellarAdmin()` (namespace `StellarAdmin`). |
| `src/StellarAdmin.UI/` | The library. Tag helpers, theming, icons, client assets. |
| `src/StellarAdmin.UI/TagHelpers/<Component>/` | One folder per component (e.g. `Sidebar/`, `Button/`, `Sheet/`). |
| `src/StellarAdmin.UI/Client/` | All client sources: TypeScript and the CSS (`css/theme.css`, `css/theme-tokens.css`, `css/components.css`, `css/anchors.css`, `css/themes/<name>.css` — the generated per-theme `.sa-*` rules), built into `src/StellarAdmin.UI/wwwroot/`. |
| `src/StellarAdmin.UI/Client/js/web-components/` | The `sel-*` Lit components. |
| `docs/DocsSamples/` | Razor Pages sample site; pages under `Pages/<Component>/` demo each component. |
| `gen/`, `util/`, gen projects | Source generators (icons) and `util/ThemeGenerator`, the manual-run console app that regenerates `Client/css/themes/*.css` from upstream shadcn styles. |
| `sandbox/` | Throwaway prototypes (e.g. `sandbox/html/*.html` for validating CSS approaches). |

Solution file: `StellarAdmin.slnx`. SDK pinned in `global.json` (`.NET 10`). Packages are centrally managed in `Directory.Packages.props`.

## Build & dev commands

### .NET
```bash
dotnet build src/StellarAdmin.UI/StellarAdmin.UI.csproj
```

### Client assets (run from `src/StellarAdmin.UI/Client/`)
```bash
npm run build                 # build:js + build:css
npm run build:js              # rolldown -> ../wwwroot/stellar-admin-ui.js  (IIFE, minified)
npm run build:css             # Tailwind v4 CLI x8 -> ../wwwroot/stellar-admin-ui.<theme>.css
npm run fmt                   # oxfmt (format TS/JS)
```
`build:css` (`Client/scripts/build-theme-bundles.mjs`) derives the theme list from `Client/css/themes/` and compiles one self-contained bundle per theme, synthesizing each entry (`css/base.css` + the theme file) over stdin — there are no checked-in per-theme entry files. **Nothing scans source files** (`base.css` uses `@import "tailwindcss" source(none)`): the bundle is fully determined by the `.sa-*` rules in `Client/css/components.css` + `css/themes/<theme>.css` and the `@source inline()` safelist at the top of `components.css` (the few utility classes tag helpers still emit as literals). If a tag helper gains a new sanctioned literal, add it to that safelist or it will silently not exist.

### Regenerating the theme CSS (manual, on shadcn updates)
```bash
dotnet run --project util/ThemeGenerator   # downloads shadcn style-*.css -> Client/css/themes/*.css
```
Adding a theme: generate its `Client/css/themes/<name>.css` and add a `ClientOutput` line for its bundle in `StellarAdmin.UI.csproj`. (Keep these as literal per-file lines — an item-transform over a name list makes Rider show the names as phantom files in the solution explorer.)

`dotnet build` runs `npm run build` for you via the `Client` target, which skips when none of its inputs changed. It hooks `ResolveProjectStaticWebAssets` rather than `Build`: `wwwroot/` is gitignored, so on a clean checkout a `BeforeTargets="Build"` target would run *after* static web asset discovery had already found the folder empty — leaving the first build with no `_content/` assets.

### Formatting
- **C#:** CSharpier (local dotnet tool, `.config/dotnet-tools.json`). Run `dotnet csharpier .` (or rely on format-on-save). Match the existing formatting in edits.
- **TS/JS:** `oxfmt` via `npm run fmt`.

## Architecture & conventions

### Tag helpers
- Inherit `StellarAdminTagHelperBase` (or `StellarAdminAnchorTagHelperBase` for anchors). Most tag helpers need no constructor at all; inject extras (e.g. `IIconManager`) when needed.
- A child tag helper can reach an ancestor via `GetParentTagHelper<TParent>()` (walks the parent stack maintained by the base). Used e.g. by `SidebarTriggerTagHelper` to read the wrapper's generated id.
- Classes are composed with `JoinCssClasses("sa-...", output.GetUserSuppliedClass())` (a static on the base class) — a plain null-skipping string join, nothing more. **Styling belongs in `Client/css/components.css` (structural) or the theme files, not in C# literals.** The only literals allowed in a merge are the sanctioned set: marker classes (`group/x`, `peer/x`, `dark`), `size-4` on icons (theme rules sniff `[class*='size-']`), `sr-only`, Field's child-width forcing, and `font-heading` — each must also be in the `@source inline()` safelist in `components.css`. Conflict resolution is the cascade's job (author utilities out-rank component rules by layer order).
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
- **Enum → data-attribute text** lives in an **extension method**, not inline `switch`/`if`. Add a `GetDataAttributeText()` in a C# 14 `extension(...)` block next to the enum. Reference: `TagHelpers/Separator/SeparatorOrientation.cs`, `TagHelpers/Sidebar/SidebarSide.cs`.
- **No default values on bound enum/bool properties.** Make them nullable and resolve at the top of `ProcessAsync`: `var effectiveSide = Side ?? SidebarSide.Left;`. Reference: `SeparatorTagHelper`, `SidebarTagHelper`.
- **Inline single-use locals** rather than naming a value used exactly once.
- **Rendering a button:** prefer calling `ButtonRenderingHelper.RenderAttributes(output, variant, size)` directly on the element you're rendering, rather than instantiating a `ButtonTagHelper` and suppressing a wrapper. Reference: `InputGroupButtonTagHelper`, `PaginationLinkTagHelper`, `SidebarTriggerTagHelper`. (`RenderAttributes` only sets `data-slot="button"` if none is already present, and folds in the user class.)

### CSS consumption model
The prebuilt per-theme bundle is the **only** consumption mode: a consumer links exactly one `_content/StellarAdmin.UI/stellar-admin-ui.<theme>.css`. There is no "single-build" mode where an app's own Tailwind build compiles the library's styles, and nothing CSS-related is shipped in the nupkg besides the bundles — no packed sources, no `.targets`.

- Each bundle is compiled from `Client/css/base.css` (`@import "tailwindcss" source(none)`, `tw-animate-css` from npm, then theme.css / shadcn-tailwind.css / anchors.css / components.css) plus one `Client/css/themes/<theme>.css`.
- **Theming customization is plain CSS**: an app redeclares the custom properties (`:root { --primary: …; --radius: … }`) in its own stylesheet — no imports needed; every compiled declaration references `var(--…)`.
- An app whose *own markup* uses token-named utilities (`bg-background`, …) adds `Client/css/theme-tokens.css` (the `@theme` vocabulary + `dark:` variant, no values) to its own Tailwind build so those utilities can be generated. External consumers copy the file (or the token block from the docs) into their project; the in-repo sample apps import it repo-relatively (see `docs/DocsSamples/Client/css/site.css`).

### Cascade layers
Tailwind's stock layer order applies (`theme, base, components, utilities`); layer names unify across same-document stylesheets, so the app's utilities out-rank the library's component rules no matter the `<link>` order. Within `components`, the generated theme rules sit in the nested `@layer components.theme`, which loses to the structural rules declared directly in `components` (see Theming above). The library emits so few utilities (the `@source inline()` safelist) that no special layer machinery is needed anymore — the old `stellar-admin` promotion layer is gone.

The Geist webfont is linked from the layout, **not** `@import`ed in CSS. A remote `@import` inside a nested imported file gets emitted after the `@layer` blocks, and browsers drop it per spec — silently, with no console error.

### Client web components (`sel-*`)
- Built with **Lit**, but rendered in **light DOM** (`createRenderRoot() { return this; }`) so server-rendered children stay styleable by Tailwind and participate in layout. Don't use shadow DOM here.
- Register a new component by adding `import "./web-components/sel-foo";` to `Client/js/stellar-admin-ui.ts`.
- **Activation uses the native Invoker Commands API**, not click handlers. A button carries `command="--custom"` + `commandfor="<id>"`; the browser dispatches a `command` event on the element with that id, and the component handles it (`addEventListener("command", ...)`, switching on `event.command`). The `interestfor` polyfill is bundled. Reference: `sel-collapsible.ts`, `sel-sidebar.ts`, and on the server side `SheetTagHelper` (auto-generates an id with `context.UniqueId` when the author didn't supply one). **Do not** invent `data-*` "marker" attributes with delegated click handlers — that pattern was tried and removed for being inconsistent with the rest of the repo.
- State is exposed to CSS by reflecting it onto `data-*` attributes that Tailwind `group-data-[...]` variants react to.

## Verifying changes
There is no unit-test project yet (xunit + `Microsoft.AspNetCore.Mvc.Testing` are available centrally for when one is added). Verify component work by running the `docs/DocsSamples` site and exercising the relevant `Pages/<Component>/` sample in the browser (desktop + mobile widths where applicable).

Both sample apps consume the prebuilt bundles, with deliberate variation — DocsSamples links the nova theme, ComponentPlayground links vega and additionally runs the `@tailwindcss/forms` plugin in its own build. Both import `theme-tokens.css` into their own Tailwind builds, keeping the token-vocabulary consumer path exercised.

### Visual-regression tool
For CSS/theming refactors that must not change rendering, `util/visual-regression/vrt.mjs` (dependency-free Node + system chromium over CDP) snapshots every DocsSamples page at two viewports — curated computed styles + rects per element (keyed by DOM path + `data-slot`, never `class`), plus overlay open-state scenarios and human-review screenshots. Capture a baseline before the risky work, re-capture after, and compare; the diff is property-level and exact. Baselines are on-demand and gitignored (`util/visual-regression/snapshots/`).
```bash
node util/visual-regression/vrt.mjs capture --url http://localhost:5205 --out util/visual-regression/snapshots/<name>   # DocsSamples must be running
node util/visual-regression/vrt.mjs compare util/visual-regression/snapshots/<baseline> util/visual-regression/snapshots/<current>
```
