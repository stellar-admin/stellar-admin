# Separate website integration

The website is an independent repository. Start website work at its root and read its `AGENTS.md` and `README.md`. Product builds, tests, and consumer reference generation do not require it.

For cross-repo work, this guide assumes a sibling `../website` checkout. Use `STELLARADMIN_WEBSITE_DIR` when the destination differs; inspect status independently in both repositories before generating anything. Website-only lint, type checking, and build use pnpm and its checked-in lockfile.

`docs/DocsSamplesGenerator` exports sample HTML, snippets, and assets into the website. Change samples and export behavior at their product source. Run `dotnet run --project docs/DocsSamplesGenerator` from the product root, with DocsSamples available as described in [development guidance](../development.md). The destination defaults to `../website` and can be overridden by `STELLARADMIN_WEBSITE_DIR`. Do not discard unrelated generated diffs automatically. The exported image upload demos have no server, so the generator injects a script into them that answers their upload requests in the browser.

Besides the demo pages and snippets, the export writes `public/demo/ui/assets/stellar-admin.theme-variables.json` (the theme variable manifest, for the theme builder's controls and the docs' theme variables page) and `assets/presets/`: one theme variable file per theme fixture plus `presets.json` (name, label, description, default first), which the theme builder's seeds read. The exported demos use the default theme and follow only the site's light/dark mode; the website has no per-demo theme picker. See [theme fixtures](stellar-admin.md#theme-fixtures) for where they come from.

For inline examples, use the [shared workflow](../design/inline-website-examples.md) and [embedding skill](../../.agents/skills/embed-website-components/SKILL.md). Their implementation lives in the website's `src/components/inline-example/` and `scripts/export-inline-example.mjs`. Run `pnpm examples:export` in the website after the relevant docs exports change, then inspect the actual diff.

The product owns shared component workflows and conventions; the website owns its application and its contributor entry point. The website remains separate throughout consolidation.
