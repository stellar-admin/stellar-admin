---
name: port-shadcn-component
description: >-
  End-to-end workflow for porting a shadcn/ui component into StellarAdmin.UI: research the
  upstream source, translate its classes into token-driven CSS, write the tag helpers, add
  DocsSamples demos, the website docs page and the generated skills reference. Use whenever adding
  or porting a component that already exists upstream — "port the X component", "add the X
  component", "add X from Base UI / shadcn / base ui", or a link to
  ui.shadcn.com/docs/components/base/X — where X is any shadcn or Base UI component name
  (bubble, message, combobox, toast, ...).
metadata:
  internal: true
---

# Porting a shadcn component

Touches the product repository and the website. Upstream settles the anatomy, the states and the default look; StellarAdmin restates that look in its tokens, so every theme follows from the theme variables without per-theme work.

For an authorized end-to-end port, carry the work through CSS, tag helpers, DocsSamples demos, website docs, and consumer references. Make routine Phase 0 decisions using the defaults below and report them. Respect a narrower user request (such as analysis only), explicit review checkpoints, and existing authorization; this skill does not authorize commits or publishing. Ask about a consequential unresolved design choice only when it blocks the next step.

Worked example — the Bubble port, as shipped: `TagHelpers/Bubble/`, `docs/DocsSamples/Pages/Bubble/`, `../website/content/docs/ui/components/bubble.mdx`.

## The fidelity rule

**Duplicate shadcn's anatomy, `data-slot` names and `data-*` state attributes exactly; translate its utility classes into token-driven CSS.** Upstream's classes are the specification of the default look, not code to copy: StellarAdmin ships no Tailwind, and a hard-coded `h-8` or `rounded-lg` would ignore density and the theme's radius. Each value becomes the token that plays the same role (Phase 2).

Verify against the **built stylesheet and the rendered page**, not the rules you wrote: compare computed sizes and colours of a DocsSamples demo, in the default theme, with upstream's.

Keep upstream behaviour that is inert for us when it costs nothing, so a later sibling component needs no CSS change (Bubble aligned itself inside a `Message` before we had built one).

## Phase 0 — settle the design

Decide these yourself and record them in the final report:

- **Polymorphic content.** Upstream renders one element polymorphically; we ship sibling tags. Default to shipping both the anchor and the button variant where upstream supports both.
- **Enums.** Dedicated per component — never reuse another's. Bubble needed four; `BubbleAlign` and `BubbleReactionsAlign` both being start/end is correct, not duplication.
- **Sibling components.** shadcn often ships a family. Default to the component asked for plus whatever it cannot render without; note the rest as follow-ups.
- **Location.** This product repository owns all product features.

## Phase 1 — research

shadcn ships three bases: `base` (Base UI), `aria` (React Aria), `radix` (Radix, published as the `new-york-v4` style). **Read the Base UI source** — it is the docs-site default and the only variant written in our two-layer shape:

```bash
curl -s https://raw.githubusercontent.com/shadcn-ui/ui/refs/heads/main/apps/v4/registry/bases/base/ui/<component>.tsx
```

It has no HTTP registry endpoint (`/r/styles/base/`, `/r/bases/base/`, `/r/@base/` all 404). The only JSON the site serves is the Radix one, worth fetching for the Phase 2 cross-check:

```bash
curl -s https://ui.shadcn.com/r/styles/new-york-v4/<component>.json | jq -r '.files[].content'
```

Read `https://ui.shadcn.com/docs/components/base/<component>` for the accessibility guidance and for **what each part is semantically for** — a grouping subcomponent usually has a specific meaning (a `MessageGroup` groups consecutive messages from *one* sender, not a whole conversation). Getting that wrong produces demos that compile and mislead.

Keep notes as you go — anatomy, prop table, behavioural notes, a11y guidance — for the docs page in Phase 5. Use a tracked plan only when the task needs continuity across sessions; keep it in `docs/plans/` and update the index.

## Phase 2 — CSS

Read the Radix JSON from Phase 1 for the look: its class strings have everything inlined (Base UI's split them behind `cn-*` markers, whose styles live in upstream's per-style CSS). Read the Base UI source for the parts and their states. List every utility per part, then restate it in tokens:

| Upstream | StellarAdmin |
|---|---|
| Spacing `p-2`, `gap-1.5`, `px-2.5` | `calc(n * var(--sa-unit))`; follows density |
| Control heights `h-6`, `h-7`, `h-8`, `h-9` | `--sa-control-h-xs`, `-h-sm`, `--sa-control-h`, `-h-lg` |
| `text-xs`, `text-sm`, `text-base` | `--sa-text-xs`, `-sm`, `-base`; control labels `--sa-text-control` |
| `rounded-md` / `rounded-lg` on controls, `rounded-xl` on containers | `--sa-radius-control-sm`, `--sa-radius-control`, `--sa-radius-container`; a part that grows taller than a control (textarea, alert) `--sa-radius-box`; tags `--sa-radius-tag`; rows in a menu or tab list `--sa-radius-item` |
| `bg-background`, `bg-card`, `bg-popover` | `--sa-color-canvas`, `--sa-color-surface`, `--sa-color-surface-raised` |
| `bg-muted`, `bg-secondary` | `--sa-color-surface-sunken`; a fill on whatever it sits on `--sa-color-tint` |
| `text-foreground`, `text-muted-foreground` | `--sa-color-ink`, `--sa-color-ink-muted` |
| `bg-primary` / `text-primary-foreground` | `--sa-color-accent-fill` / `--sa-color-on-accent`; accent as text `--sa-color-accent-text` |
| `bg-accent` (hover), selected | `--sa-color-hover`, `--sa-color-selected`, `--sa-color-highlight` / `--sa-color-on-highlight` |
| `border`, `border-input` | `--sa-border-width` with `--sa-color-line` (containers, dividers) or `--sa-color-line-control` (controls) |
| `destructive` | `--sa-color-danger-fill`, `--sa-color-on-danger`, `--sa-color-danger-text`, `--sa-color-danger-soft` |
| `shadow-xs`, `shadow-sm`, `shadow-md`, `shadow-lg` | `--sa-shadow-control`, `-container`, `-overlay`, `-dialog` |
| `focus-visible:ring-*`, `aria-invalid:*` | `--sa-focus-shadow` (buttons, tabs) or `--sa-focus-shadow-field` (inputs); `--sa-invalid-shadow` |
| `font-medium` on controls | `--sa-weight-control` |
| `disabled:opacity-50` | `--sa-disabled-opacity` |

[Theme tokens](../../../docs/design/tokens.md) lists every name; use nothing else, and nothing private to another component. Where a value has no role (an icon gap, a 1px nudge), keep it as a multiple of `--sa-unit` or a literal, and say so in a comment. The Tailwind adapter (`Client/css/tailwind-adapter.css`) maps the same names the other way and is a quick cross-check.

Write the rules in `Client/css/components/<component>.css`: a header comment naming the component and its tags, the rules in `@layer sa.components`, and private `--_*` variables for values that vary by variant, size or state (the variant classes set them; the base rule reads them). Values that differ in dark mode go in `@layer sa.tokens` under `.dark`. Style off the `data-*` attributes and native state, never marker classes. A file must not rely on another file's order: a rule that adjusts another component's part out-ranks it by selector. Follow an existing file of the same kind (`badge.css`, `button.css`, `card.css`).

When upstream's look needs a decision the tokens don't make (an outline only some themes want, a band behind a header), check the theme variables first: `--sa-outlines`, `--sa-footer-fill`, `--sa-column-head-fill` and the rest already switch such treatments across components, and a new part should honour the ones that apply to it. A treatment that must differ between themes and no theme variable covers is a new theme variable, which needs Jerrie's approval; see the [theme skill](../create-custom-theme/SKILL.md). Don't add one just for this component.

Build the CSS directly; it fails loudly where `dotnet build` can succeed over a silently failed CSS build:

```bash
cd src/StellarAdmin.UI/Client && npm run build:css
```

Then check the component in DocsSamples (Phase 4) in the default theme against upstream, and in the theme fixtures (`?theme=ledger|ops|soft`, light and dark), which move radius, density, outlines and elevation the furthest. Run `util/theme-check/contrast.mjs` on its page (see its README): no text failures, and no new faint parts or pill corners.

## Phase 3 — tag helpers

`src/StellarAdmin.UI/TagHelpers/<Component>/`, one file per type. Pattern reference: `TagHelpers/Bubble/`.

**Naming: component prefix first, element role last** — `sa-bubble-content-link`, `sa-sidebar-menu-button`, `sa-item-link`, with class names to match. The link version of a part appends `-link` to the part's tag, as in `sa-button-link` and `sa-command-item-link`.

- Nullable enum properties resolved at the top of `ProcessAsync`: `var effectiveVariant = Variant ?? BubbleVariant.Default;`
- `GetDataAttributeText()` on each enum in a C# 14 `extension(...)` block; the extension class is `internal` and gets no XML docs.
- `data-slot` on the primary element, plus `data-*` for every piece of state the CSS keys off.
- `JoinCssClasses(..., output.GetUserSuppliedClass())` — user classes last so they win.
- **Synchronous** `ProcessAsync` returning `Task.CompletedTask` unless you relocate child content. Never call `GetChildContentAsync` in a decorate-only helper — content renders automatically, and fetching without writing it back drops it.
- Shared `<Component>ContentRenderingHelper` when several tags render one surface as different elements (`BubbleContentRenderingHelper`, `ItemRenderingHelper`).
- Anchors derive `StellarAdminAnchorTagHelperBase`, inject `IHtmlGenerator`, and `await ApplyRouteAttributesAsync(...)` before rendering for the full `asp-*` set.
- Buttons set `type="button"` when the author has not, so one inside a form does not submit it.
- Member ordering, logical-section spacing, and mandatory `if`/`else` braces per `docs/conventions/csharp-file-organization.md`; XML docs per `docs/conventions/xml-documentation.md` (brief consumer-facing wording in multiline indented `<summary>`/`<remarks>` blocks, no mention of shadcn). Do not confuse a one-sentence summary with single-line XML formatting; CSharpier alone does not enforce these rules.
- Share IDs and resolved options through an internal sealed `<Component>Context`, published with `SetContext` before children render and read with `GetContext<T>` in children. Use required init-only context properties; resolve defaults once in the parent. Do not expose internal parent Tag Helper properties merely for children to read via `GetParentTagHelper`. Follow the context guidance in `docs/repos/stellar-admin.md`; `CarouselContext` is a reference.

Build, then format only the folder you touched. Expect 0 warnings.

```bash
dotnet build src/StellarAdmin.UI/StellarAdmin.UI.csproj
# From the product repository root, after dotnet tool restore:
dotnet csharpier format src/StellarAdmin.UI/TagHelpers/<Component>
```

## Phase 4 — DocsSamples

`docs/DocsSamples/Pages/<Component>/` — `Index.cshtml`, `Index.cshtml.cs`, one `_<Name>.cshtml` per demo. Copy the scaffolding from `Pages/Bubble/`.

- Wrap the snippet the docs should publish in `<!-- code begin -->` / `<!-- code end -->`. **The published snippet must be copy-pasteable on its own** — if the component needs a layout container to work (the flex wrapper a message thread sits in), that container goes *inside* the markers. Only page chrome the reader doesn't need stays outside.
- Content uses the **Voyager Travel** theme.
- Register the page in `Pages/Shared/_NavigationLayout.cshtml`, right `DemoGroup`, alphabetically. VRT needs no registration — it lists the `Pages/` directory.

**Demos are compositions, not specimen strips.** Show the component doing a job a reader would actually give it; demo an interactive variant in the context that makes it obviously the right choice. Bubble's link and button demos had to be rebuilt from isolated bubbles into realistic threads.

**Use each subcomponent for what it means**, per the Phase 1 notes — a demo that nests things in a structurally valid but semantically wrong way teaches the wrong API.

```bash
ASPNETCORE_ENVIRONMENT=Development \
  dotnet run --project docs/DocsSamples --no-launch-profile --urls http://localhost:5206
```

Check desktop and mobile widths, light and dark (`?mode=dark`), in the default theme and the fixtures (`?theme=ops`, `?theme=soft`). **Stop the server when done.** Port 5205 is Jerrie's own instance; never touch it.

## Phase 5 — website docs

Register each partial in `tools/StellarAdmin.Cli/Docs/DemoGenerator.cs` as a `new("<Component>/_<Name>")` entry in `DemoPartials`, alphabetically, then run it:

```bash
dotnet run --project tools/StellarAdmin.Cli -- docs generate demos
```

The generator regenerates all demos, and SVG attribute order plus antiforgery tokens can differ between runs. Record pre-existing edits first. Run `bash .agents/skills/port-shadcn-component/scripts/classify-generator-output.sh ../website` from the product repository for a read-only classification, then review the actual diffs. Normalized equality can hide real SVG edits and must never trigger automatic restores. Use isolated worktrees or a pre-generation snapshot if output cleanup is needed; preserve existing edits. Check that the exported `stellar-admin.css` changed only by the new component's rules.

Write `../website/content/docs/ui/components/<component>.mdx` by hand and add its slug to `meta.json` under the right group. Structure, per `bubble.mdx`:

- Frontmatter `title` / `description`, then `import { Demo } from "@/components/demo";`
- Intro demo → `## Usage` with the composition skeleton → `## Examples`, one `###` per demo (prose paragraph + `<Demo>` + `<include>`).
- Fold the a11y guidance into the relevant example section, not a separate heading.
- `## API Reference`, one `###` per tag in **anatomical** order (root then children, not alphabetical), each with a `<TypeTable>`.
- Anchor-backed tags get `<include>./_shared/routing-attributes.mdx</include>` under `#### Routing attributes`.
- Prose: bracketed `<sa-thing>` for tags, bare for attributes and classes, "Tag Helper" capitalised, `...` never `…`, **no hard wrapping**.

Then run `pnpm lint`, `pnpm types:check`, and `pnpm build` from `../website/`.

## Phase 6 — skills reference

Add curated examples to `tools/StellarAdmin.Cli/Skills/skills.examples.json` — the partials that best teach the component, including at least one real composition. Paths must match the `Pages/` tree; missing files are skipped **silently**.

```bash
dotnet run --project tools/StellarAdmin.Cli -- skills generate
dotnet run --project tools/StellarAdmin.Cli -- skills check   # must report no drift
```

Under `skills/stellar-admin-ui/references/`, `components/*.md` and `components-index.md` are generated from the UI library. Only marked `<!-- structure:begin -->` / `<!-- structure:end -->` regions inside component files are hand-authored and preserved. The other reference guides are handwritten; keep them consistent with the public docs.

## Phase 7 — verify and hand over

- No unresolved `sa-*` elements in the rendered HTML. A tag helper that fails to match leaks its custom element into the output, and nothing else will tell you.
- `dotnet build` clean with 0 warnings; `pnpm lint` clean; `skills check` reports no drift.
- Screenshot the DocsSamples page and the built docs page.
- Every server stopped, ports free.

Report, in one message: what shipped in each repo, the Phase 0 decisions you took, **every deviation from upstream with its reasoning**, and anything deferred. Then list what is uncommitted per repo, and stop. Commit or push only if the user has authorized that action in the conversation; completing the component alone does not authorize publishing changes.

## Environment and instructions

Read the product `AGENTS.md`, the relevant repository guide under `docs/repos/`, and [development guidance](../../../docs/development.md) before executing commands. Paths in the phases are product-root-relative unless a working directory is stated. That guide records conditional SDK, IPv6, prune-data, formatter, and sample-server workarounds; do not assume they apply on every machine. Use the pinned SDK and local tools where available.
