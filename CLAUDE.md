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

`AddStellarAdmin(Action<StellarAdminBuilder>)` is also available. `AddUI()` is what registers `TwMerge`, `ICssClassMerger` and `IIconManager`, and applies the Lucide icon pack default — `AddStellarAdmin()` on its own does not pull in the UI services. Theme selection is **not** part of registration: it's whichever theme stylesheet the app links.

## Repository layout

| Path | What |
|------|------|
| `src/StellarAdmin.Core/` | Shared DI entry point — `StellarAdminBuilder`, `AddStellarAdmin()` (namespace `StellarAdmin`). |
| `src/StellarAdmin.UI/` | The library. Tag helpers, theming, icons, client assets. |
| `src/StellarAdmin.UI/TagHelpers/<Component>/` | One folder per component (e.g. `Sidebar/`, `Button/`, `Sheet/`). |
| `src/StellarAdmin.UI/Theming/` | `ClassElement` / `ClassList` / `ThemeToken` — the mergeable class abstraction. |
| `src/StellarAdmin.UI/Client/` | TypeScript + the two-bundle CSS entry point, built into `src/StellarAdmin.UI/wwwroot/`. |
| `src/StellarAdmin.UI/Client/js/web-components/` | The `sel-*` Lit components. |
| `src/StellarAdmin.UI/tailwind/` | Canonical Tailwind sources (theme vars, variants, anchors, vendored `tw-animate-css`) **and `themes/<name>.css`** — the generated per-theme `.sa-*` component rules. Consumed by our own build **and** packed for single-build mode. |
| `src/StellarAdmin.UI/build/` | `StellarAdmin.UI.targets`, shipped in the package to copy `tailwind/` into a consumer's `obj/`. |
| `docs/DocsSamples/` | Razor Pages sample site; pages under `Pages/<Component>/` demo each component. |
| `gen/`, `util/`, gen projects | Source generators (icons) and `util/ThemePackGenerator`, the manual-run console app that regenerates `tailwind/themes/*.css` from upstream shadcn styles. |
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
npm run sync:vendor           # refresh tailwind/vendor/ (only after a dep bump)
npm run fmt                   # oxfmt (format TS/JS)
```
`build:css` (`Client/scripts/build-theme-bundles.mjs`) compiles one self-contained bundle per entry in `Client/css/themes/`. **Nothing scans source files** (`theme-bundle-base.css` uses `@import "tailwindcss" source(none)`): the bundle is fully determined by the `.sa-*` rules in `tailwind/components.css` + `tailwind/themes/<theme>.css` and the `@source inline()` safelist at the top of `components.css` (the few utility classes tag helpers still emit as literals). If a tag helper gains a new sanctioned literal, add it to that safelist or it will silently not exist.

### Regenerating the theme CSS (manual, on shadcn updates)
```bash
dotnet run --project util/ThemePackGenerator   # downloads shadcn style-*.css -> tailwind/themes/*.css
```
Adding a theme: generate its `tailwind/themes/<name>.css`, add a `Client/css/themes/<name>.css` entry, and add a `ClientOutput` line for its bundle in `StellarAdmin.UI.csproj`. (Keep these as literal per-file lines — an item-transform over a name list makes Rider show the names as phantom files in the solution explorer.)

`dotnet build` runs `npm run build` for you via the `Client` target, which skips when none of its inputs changed. It hooks `ResolveProjectStaticWebAssets` rather than `Build`: `wwwroot/` is gitignored, so on a clean checkout a `BeforeTargets="Build"` target would run *after* static web asset discovery had already found the folder empty — leaving the first build with no `_content/` assets.

### Formatting
- **C#:** CSharpier (local dotnet tool, `.config/dotnet-tools.json`). Run `dotnet csharpier .` (or rely on format-on-save). Match the existing formatting in edits.
- **TS/JS:** `oxfmt` via `npm run fmt`.

## Architecture & conventions

### Tag helpers
- Inherit `StellarAdminTagHelperBase` (or `StellarAdminAnchorTagHelperBase` for anchors). Inject `ICssClassMerger` via the base constructor; inject extras (e.g. `IIconManager`) as needed.
- A child tag helper can reach an ancestor via `GetParentTagHelper<TParent>()` (walks the parent stack maintained by the base). Used e.g. by `SidebarTriggerTagHelper` to read the wrapper's generated id.
- Classes are composed with `ClassMerger.Merge(new ThemeToken("sa-..."), output.GetUserSuppliedClass())` — a join + de-dupe, nothing more. **Styling belongs in `tailwind/components.css` (structural) or the theme files, not in C# literals.** The only literals allowed in a merge are the sanctioned set: marker classes (`group/x`, `peer/x`, `dark`), `size-4` on icons (theme rules sniff `[class*='size-']`), `sr-only`, Field's child-width forcing, and `font-heading` — each must also be in the `@source inline()` safelist in `components.css`. Conflict resolution is the cascade's job (author utilities out-rank component rules by layer order).
- `output.GetUserSuppliedClass()` (extension in `TagHelpers/TagHelperOutputExtensions.cs`) reads the author's `class` (read-only; leaves it on `output`).
- Emit a `data-slot="..."` on the primary element (shadcn convention; also used as a styling/query hook).

### Theming & component styling
- **A theme is a stylesheet.** Themed declarations live as `.sa-*` rules in `tailwind/themes/<theme>.css` (generated by `util/ThemePackGenerator` from upstream shadcn styles, wrapped in the nested `@layer components.theme`). The theme-independent structural half lives in the hand-maintained `tailwind/components.css` under the same class names, directly in `@layer components`. Nothing resolves classes server-side.
- **Precedence, lowest to highest:** theme rules (`components.theme` sublayer) < structural rules (directly in `components` — styles outside a nested layer beat the nested layer) < utilities. So structure wins same-property conflicts with a theme, and an author's `class` utilities beat everything.
- `new ThemeToken("sa-...")` passed to `ClassMerger.Merge` emits that name as a **literal class** on the element. A token with no rule renders as a harmless dead class.
- State/variant styling keys off the `data-*` attributes tag helpers emit (`[data-side]`, `[data-orientation]`, `[data-anchor-side]`, …) as attribute selectors inside the component rules — never off extra marker classes.
- One marker-class exception: `MenuColor.Inverted` emits the literal `dark` class (re-scopes theme variables on the element), which a stylesheet rule cannot express.
- **Utilities that override *other elements'* classes must stay in the class attribute** (utilities layer), e.g. Field's `[&>*]:w-full` — a components-layer rule cannot reliably beat a child's own component classes.

### Established C# patterns (follow these — they're enforced in review)
- **Enum → data-attribute text** lives in an **extension method**, not inline `switch`/`if`. Add a `GetDataAttributeText()` in a C# 14 `extension(...)` block next to the enum. Reference: `TagHelpers/Separator/SeparatorOrientation.cs`, `TagHelpers/Sidebar/SidebarSide.cs`.
- **No default values on bound enum/bool properties.** Make them nullable and resolve at the top of `ProcessAsync`: `var effectiveSide = Side ?? SidebarSide.Left;`. Reference: `SeparatorTagHelper`, `SidebarTagHelper`.
- **Inline single-use locals** rather than naming a value used exactly once.
- **Rendering a button:** prefer calling `ButtonRenderingHelper.RenderAttributes(output, ClassMerger, variant, size)` directly on the element you're rendering, rather than instantiating a `ButtonTagHelper` and suppressing a wrapper. Reference: `InputGroupButtonTagHelper`, `PaginationLinkTagHelper`, `SidebarTriggerTagHelper`. (`RenderAttributes` only sets `data-slot="button"` if none is already present, and folds in the user class.)

### The two CSS entry points
The Tailwind sources in `tailwind/` are canonical and shared by both consumption modes. Nothing is copied between two checked-in locations — edit `tailwind/theme.css` and both modes pick it up.

- **`tailwind/index.css` + `tailwind/themes/<theme>.css`** — single-build mode. A consuming app imports both from its own Tailwind entry (index first, then the theme of its choice), and its build emits one bundle covering both the app and the library. `index.css` deliberately does **not** `@import "tailwindcss"` (the consumer already did).
- **`Client/css/themes/<theme>.css`** — two-bundle mode (the default). Each entry imports `Client/css/theme-bundle-base.css` (`@import "tailwindcss" source(none)` + `tailwind/index.css`) plus its `tailwind/themes/<theme>.css`, and builds the prebuilt `wwwroot/stellar-admin-ui.<theme>.css`. The consumer links exactly one. Two-bundle apps that use theme-token utilities in their own markup (`bg-background`, `text-muted-foreground`, …) must import `tailwind/theme.css` into their own build so those utilities can be generated (see `docs/DocsSamples/Client/css/site.css`).

Everything in `tailwind/` must be resolvable **without our `node_modules`**, since a consumer's build reads these files directly. That's why `tw-animate-css` is vendored into `tailwind/vendor/` (committed, refreshed by `npm run sync:vendor`) and why the `@toolwind/anchors` plugin was replaced by `tailwind/anchors.css`. **Don't add a `@plugin` or a bare package `@import` to anything under `tailwind/`** — it will work in our build and silently break every single-build consumer.

### Cascade layers
Both modes use Tailwind's stock layer order (`theme, base, components, utilities`); layer names unify across same-document stylesheets, so in two-bundle mode the app's utilities out-rank the library's component rules no matter the `<link>` order. Within `components`, the generated theme rules sit in the nested `@layer components.theme`, which loses to the structural rules declared directly in `components` (see Theming above). The library emits so few utilities (the `@source inline()` safelist) that no special layer machinery is needed anymore — the old `stellar-admin` promotion layer is gone.

The Geist webfont is linked from the layout, **not** `@import`ed in CSS. A remote `@import` inside a nested imported file gets emitted after the `@layer` blocks, and browsers drop it per spec — silently, with no console error.

### Client web components (`sel-*`)
- Built with **Lit**, but rendered in **light DOM** (`createRenderRoot() { return this; }`) so server-rendered children stay styleable by Tailwind and participate in layout. Don't use shadow DOM here.
- Register a new component by adding `import "./web-components/sel-foo";` to `Client/js/stellar-admin-ui.ts`.
- **Activation uses the native Invoker Commands API**, not click handlers. A button carries `command="--custom"` + `commandfor="<id>"`; the browser dispatches a `command` event on the element with that id, and the component handles it (`addEventListener("command", ...)`, switching on `event.command`). The `interestfor` polyfill is bundled. Reference: `sel-collapsible.ts`, `sel-sidebar.ts`, and on the server side `SheetTagHelper` (auto-generates an id with `context.UniqueId` when the author didn't supply one). **Do not** invent `data-*` "marker" attributes with delegated click handlers — that pattern was tried and removed for being inconsistent with the rest of the repo.
- State is exposed to CSS by reflecting it onto `data-*` attributes that Tailwind `group-data-[...]` variants react to.

## Verifying changes
There is no unit-test project yet (xunit + `Microsoft.AspNetCore.Mvc.Testing` are available centrally for when one is added). Verify component work by running the `docs/DocsSamples` site and exercising the relevant `Pages/<Component>/` sample in the browser (desktop + mobile widths where applicable).

The two sample apps deliberately run in **different modes**, so both paths stay exercised on every build — don't "consistency-fix" them into agreement:
- `docs/DocsSamples` — two-bundle mode (links the prebuilt `_content/StellarAdmin.UI/stellar-admin-ui.nova.css`).
- `sandbox/ComponentPlayground` — single-build mode (its own Tailwind build importing `obj/stellaradmin-ui/tailwind/index.css` + `themes/vega.css`; no `_content` link).

If you change anything under `tailwind/`, check the generated bundles still agree. The cheapest meaningful check is a **selector-set diff** between `src/StellarAdmin.UI/wwwroot/stellar-admin-ui.vega.css` and `sandbox/ComponentPlayground/wwwroot/css/site.css` — the app's set should be a superset. Visual inspection of ComponentPlayground proves little; its Index page has one component.

### Visual-regression tool
For CSS/theming refactors that must not change rendering, `util/visual-regression/vrt.mjs` (dependency-free Node + system chromium over CDP) snapshots every DocsSamples page at two viewports — curated computed styles + rects per element (keyed by DOM path + `data-slot`, never `class`), plus overlay open-state scenarios and human-review screenshots. Capture a baseline before the risky work, re-capture after, and compare; the diff is property-level and exact. Baselines are on-demand and gitignored (`util/visual-regression/snapshots/`).
```bash
node util/visual-regression/vrt.mjs capture --url http://localhost:5205 --out util/visual-regression/snapshots/<name>   # DocsSamples must be running
node util/visual-regression/vrt.mjs compare util/visual-regression/snapshots/<baseline> util/visual-regression/snapshots/<current>
```
