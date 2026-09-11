# Visual verification

Start a built DocsSamples app on an available port. Capture a theme at desktop and mobile widths with the same browser and fonts for both revisions:

```bash
node util/visual-regression/vrt.mjs capture --url http://localhost:5206 --theme ledger --mode light --out snapshots/ledger-light
node util/visual-regression/vrt.mjs capture --url http://localhost:5206 --theme ledger --mode dark --out snapshots/ledger-dark
node util/visual-regression/vrt.mjs compare snapshots/base snapshots/head --out snapshots/diff
```

The default theme is Nova and the default mode is light. Pass `--browser google-chrome` when Chromium is installed under that name. Keep captures outside source control; the PR workflow produces its own base/head captures and report.

Run Ledger's focused interaction and style checks with `node util/visual-regression/verify-ledger.mjs http://localhost:5206`. Set `CHROME_PATH` to choose a browser binary. CI runs this check against the head app. It verifies slider keyboard operation, switch state, OTP entry, composite field focus, pressed/focused buttons, radius customization, reduced motion, and body/primary text contrast in both modes. It complements visual inspection; it is not a comprehensive accessibility audit.

Theme inventory and explicit component coverage are checked separately by `node util/theme-coverage/check.mjs`.
