# Theme coverage

Run `node util/theme-coverage/check.mjs` from the repository root, or `npm run check:themes` from the library Client directory. The check also runs before CSS bundles are built and in the visual-regression workflow. Run the failure-case tests with `node --test util/theme-coverage/check.test.mjs`.

`coverage.json` records the shipped component families, public tags, styling hooks, representative examples, and an explicit decision for every theme. It is maintained source, not an output to regenerate automatically when a check fails.

When a component, subcomponent, variant hook, or theme is added:

1. Implement the component's structural rules and each theme's visual rules. For custom themes, compose the existing surface, control, typography, and interaction treatments.
2. Add or extend a real DocsSamples example and verify the relevant states in light/dark mode at desktop/mobile widths.
3. Update the component's `tags` and `hooks` lists and each theme's coverage entry. `reviewed` requires the names of actual theme rules; `shared-only` requires a rationale explaining which shared or composed styles provide the appearance. `pending` is deliberately rejected by normal builds.
4. Run coverage validation and the CSS build. A new theme also requires its `ClientOutput` entry in the library project.

The checker detects missing declarations, inventory changes, and removed rule evidence. It does not judge visual quality or prove that every CSS state is correct. A review must inspect the implementation and rendered examples before updating the manifest; do not satisfy a failure with placeholder rules.

Ice, Concourse, Ledger and Observatory are hand-authored in `Client/css/themes/ice.css`, `Client/css/themes/concourse.css`, `Client/css/themes/ledger.css`, and `Client/css/themes/observatory.css`. ThemeGenerator owns only the upstream-derived theme files. Its hardcoded source list must never include Concourse, Ledger, Ice or Observatory. Consumer applications select a theme by linking its bundle; there is no separate tag helper API.

Observatory is hand-authored in `Client/css/themes/observatory.css` and uses compact density. Verify it with `node util/visual-regression/verify-observatory.mjs http://localhost:5206`; optionally set `OBSERVATORY_PRO_URL=http://localhost:5207` to include Pro grid containment and `OBSERVATORY_FONT_CSS` to a local CSS file with embedded Plex font faces for deterministic captures.
