# Visual verification

Start a built DocsSamples app on an available port. Capture a theme at desktop and mobile widths with the same browser and fonts for both revisions:

```bash
node util/visual-regression/vrt.mjs capture --url http://localhost:5206 --theme ledger --mode light --out snapshots/ledger-light
node util/visual-regression/vrt.mjs capture --url http://localhost:5206 --theme ledger --mode dark --out snapshots/ledger-dark
node util/visual-regression/vrt.mjs compare snapshots/base snapshots/head --out snapshots/diff
```

Without `--theme` the capture uses DocsSamples' default theme; `--theme` takes a preset (`ledger`, `ops`, `soft`). The default mode is light. Pass `--browser google-chrome` when Chromium is installed under that name. Keep captures outside source control; the PR workflow produces its own base/head captures and report.

Text contrast and screenshots across presets and random themes are in [theme checks](../theme-check/README.md).
