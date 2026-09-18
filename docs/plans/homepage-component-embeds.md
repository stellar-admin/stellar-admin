# Homepage component embeds

## Code audit — 2026-09-18

Current status: **proposed**. The website retains `src/components/inline-example/inline-example.tsx`, generated fragments and `scripts/export-inline-example.mjs`, but `src/routes/index.tsx` does not consume the wrapper. The experiment is complete; actual homepage integration and a general Razor fragment export remain unimplemented. Current workflow: `docs/design/inline-website-examples.md`; current generator: `docs/DocsSamplesGenerator/Generator.cs`. Website inspected read-only.

This assessment uses the current checkout; earlier status, paths, permissions and verification notes below describe historical sessions. Runtime/browser and hosted release checks were not rerun for this documentation audit.

## Historical record

Status: reference — experiment accepted; reusable implementation retained, experimental route and standalone spike removed; homepage integration remains proposed.

Last updated: 2026-09-14. Affected repos: workspace and website. OSS and Pro remain unchanged. Current implementation guidance is maintained in [inline website examples](../design/inline-website-examples.md).

Recommend rendering selected StellarAdmin examples directly into the homepage as generated HTML, with one shared browser runtime and a website-specific scoped stylesheet. Keep React responsible for the page layout and marketing interactions; let StellarAdmin own behavior inside each example. This retains the actual product components while allowing normal responsive layout, automatic content height and overlays that belong to the page.

## Existing architecture

- `website/src/routes/index.tsx` renders a React hero inside Fumadocs `HomeLayout`. The website uses React 19.2.8, TanStack Start, Tailwind 4 and Base UI. `vite.config.ts` enables prerendering and Vercel output.
- `website/src/components/demo-preview.tsx` embeds complete generated documents and observes each iframe body to synchronize height.
- `stellar-admin-pro/docs/DocsSamplesGenerator/Generator.cs` already renders Razor examples and exports their HTML, assets and source snippets. Extend this export pipeline for homepage fragments; a visitor does not need a running .NET server.
- StellarAdmin emits `.sa-*` classes and light-DOM `sel-*` custom elements, activated by the exported IIFE runtime. Some controls use browser behavior without custom elements. For example, the current tabs intro is navigation markup, not a client-side panel-switching widget.
- Website and library Observatory palettes are aligned, but website typography and component recipes are separate. Library CSS includes Tailwind preflight, global `html`/`body` rules, `:root`/`.dark` tokens and utility classes.

React supports custom HTML elements, and its raw-HTML facility can host trusted generated markup. This fits the existing architecture without a framework migration. See [React DOM components](https://react.dev/reference/react-dom/components) and [raw HTML](https://react.dev/reference/react-dom/components/common#dangerously-setting-the-inner-html).

## Browser experiment

The initial standalone browser harness used existing exported HTML and assets in a temporary server and isolated browser. It was removed after the successful website integration superseded it; the following findings are retained as historical evidence, not instructions to run a deleted script.

The initial harness used the installed React server renderer and browser hydration, the website's compiled application CSS, and three existing exports. It loaded the library runtime from an effect after hydration and treated each HTML fragment as an opaque subtree. The subsequent route experiment covered TanStack integration as recorded below.

Verified in Chromium 151.0.7922.173:

| Check | Result |
| --- | --- |
| Server-rendered fragments hydrate | No hydration warnings or recoverable errors |
| Inline collapsible | Opens; trigger `aria-expanded` becomes `true` |
| Inline dropdown | Opens; focus moves to a menu item |
| Inline dialog | Enters native modal top layer; body scrolling locks and restores on close |
| React unmount/remount | Collapsible works again; exactly one runtime script |
| Full library CSS added to website CSS | Changes the document font from the website system stack to IBM Plex Sans |
| Same collapsible inside Shadow DOM | Opens, but trigger `aria-expanded` incorrectly remains `false` |

The Shadow DOM failure follows directly from `sel-collapsible.ts` using `document.querySelectorAll` to find its triggers. Dropdown menus also use document-wide trigger, submenu and active-element lookups. Input OTP installs a document stylesheet. A shadow wrapper would require component and polyfill auditing, root-aware queries, focus handling and adapted theme selectors. Light DOM is an intentional library choice; [Lit documents both rendering roots](https://lit.dev/docs/components/shadow-dom/).

Measured current exports: runtime 47,644 bytes raw / 15,003 gzip; Observatory CSS 194,221 bytes raw / 26,092 gzip. These exclude React, website CSS, fonts, markup and sample utilities. Iframes can already share cached asset downloads; the expected improvement is fewer documents, one runtime execution and less layout/coordination overhead. No comparative performance benchmark was run.

## Options

| Approach | Fit and tradeoff |
| --- | --- |
| Generated HTML in light DOM, shared runtime, scoped CSS | Recommended for real product demonstrations. Preserves Razor as source of truth and naturally participates in homepage layout. Requires CSS export work and lifecycle verification. |
| Handwritten JSX using `.sa-*` and `sel-*` | Useful for a few highly composed examples. React 19 supports custom elements, but this duplicates Tag Helper output and needs synchronization and careful DOM ownership. |
| React/Base UI components | Best for homepage controls such as a pricing switch or showcase selector. Already installed and integrated with site styling. Recreating product demos this way introduces a second implementation. |
| Shadow DOM wrapper | Provides style encapsulation, but current runtime assumptions demonstrably break. Higher effort than its apparent simplicity suggests. |
| Iframes | Retain for docs, complete application previews, and independently themed showcases where isolation matters. Full application/server behavior still needs an appropriate backend or explicit demo simulation. |

## Proposed integration

1. Author curated homepage examples in Razor, then export layout-free fragments alongside an asset manifest. Exclude the docs preview padding, minimum-height wrappers, document scripts and theme-storage synchronization. Export an explicit initializer when an example requires custom scripting; inserting HTML does not execute its scripts. Keep website CI able to build from checked-in exports without the sibling .NET projects.
2. Expose a small React API such as `<ComponentExample name="booking-controls" instanceId="hero-booking" />`. Include the fragment in server/prerender output rather than fetching each demo after page load. Use identical markup during hydration. Treat its descendants as owned by StellarAdmin; React can control the surrounding layout and communicate through DOM events or public methods.
3. Load the runtime once on the client after hydration, with shared loading/error state. Do not import the current browser IIFE into SSR: it accesses `window` and registers custom elements. Keep the registration for subsequent route visits. Native upgrades activate newly inserted elements, as the remount experiment confirms for the tested collapsible.
4. Generate website-owned embed CSS from the existing compiled theme, retaining the public library's prebuilt-bundle consumption model. Scope component rules, required utilities and theme variables to an example wrapper; handle `:root`, `html`, `body`, `.dark`, layer ordering, keyframes and `@property` explicitly. Use a CSS parser, not a regex prefixer. Include the selected samples' layout utilities, which are not all in the library bundle. Do not simply load the entire DocsSamples `site.css` into the homepage.
5. Start with one homepage theme and follow the website's dark-mode state. Multiple simultaneous theme previews need independently scoped palettes and component rules, not just different token values. A scoped export might use selector rewriting or [CSS `@scope`](https://developer.mozilla.org/en-US/docs/Web/CSS/Reference/At-rules/@scope); neither is a complete solution for global at-rules or incoming site styles. A temporary selector-based adapter is implemented and was verified for the selected Observatory examples; general theme support remains unimplemented.
6. Give every example instance deterministic unique IDs and update their references together: `commandfor`, `popovertarget`, labels, ARIA ID lists and anchor references. Export once per known homepage placement or use a parser-based transformation with the same instance key on server and client. Repeating the current export verbatim can create collisions.

## Website experiment and cleanup

The follow-up example demonstrated two independent collapsibles and a dialog in a TanStack route. The user accepted the experiment and requested retaining its reusable code without publishing the example page. The experimental route and its generated route-tree entries were removed before committing. `website/src/components/inline-example/inline-example.tsx` owns the React integration; its README explains the code. `website/scripts/export-inline-example.mjs` generates scoped Observatory CSS and fragments from existing docs exports using parse5/PostCSS. This is an example-specific export adapter, not the proposed Razor pipeline extension. The original homepage and docs embeds are unchanged.

Verified for this route in Chromium: no iframes, one runtime script, no hydration errors or duplicate IDs; independent expansion with automatic page height; modal open/close and scroll cleanup on navigation; browser history return with working controls; matching document/nav fonts, heading size and root radius with the embed stylesheet enabled/disabled; dark token inheritance and no mobile horizontal overflow. Website lint, type checking and production build passed. Dependency installation used the pinned pnpm 10.33.0 with a command-local `--config.engine-strict=false` because its embedded Node reports 20.11.1; generation and website scripts ran using the installed system Node 26.7.0. No user-wide configuration changed.

Build one actual homepage section containing an inline form/collapsible, dropdown and dialog using scoped CSS. Verify desktop/mobile and light/dark visuals alongside Fumadocs navigation/search; keyboard operation, Escape and focus restoration; repeated examples; hard load and client navigation away/back; and navigation while a modal is open. The current dialog component mutates `document.body` and observes the dialog without disconnect cleanup, so modal removal and interaction with Base UI scroll locking need particular attention.

The original isolated spike did not verify scoped styling or TanStack navigation; the follow-up covers the selected examples as recorded above. Remaining coverage includes other components and browsers, combined Base UI/StellarAdmin modal flows, simultaneous themes and Pro/server-backed components. The next homepage step is to choose the actual sections and examples, then replace this example-specific export adapter with curated Razor exports. A complete homepage redesign remains outside this spike.

The retained code is documented in its website README and the maintained workspace workflow. The workspace `embed-website-components` development skill routes future work to those instructions; it is available through the canonical `.agents/skills/` folder and the Claude symlink. No public consumer skill is needed for this website-only integration.

## Final retained state and verification

Website commit: `5d834a13` (`master`) retains the wrapper, generated fragments/theme/manifest, temporary exporter, dependencies and usage README. The example route and standalone spike were never committed. The generated route tree matches its original state. Workspace documentation and the development skill accompany that commit in the independent workspace repo; nothing was pushed or deployed.

After cleanup, `pnpm examples:export`, `pnpm lint`, `pnpm types:check` and `pnpm build` passed. Production output includes the ordinary homepage and excludes `/examples/inline-components`; application JS/CSS contains no inline wrapper/theme markers, and the homepage HTML does not reference the demo runtime. A Chromium check against the dev site confirmed the original homepage heading, zero inline wrappers/runtime scripts, only ordinary page stylesheets and HTTP 404 for the removed route.

Skill validation and local documentation-link checks passed; the Claude symlink resolves to the canonical skill. Fresh-session agent discovery was not rerun. Retained generated snippets can contribute ordinary utilities to the website's Tailwind scan, but their scoped theme and JavaScript are not included in page bundles without a consumer. Future homepage integration and general exporter support remain separate work.
