# Visual verification

Start a built DocsSamples app on an available port. Capture a theme at desktop and mobile widths with the same browser and fonts for both revisions:

```bash
node util/visual-regression/vrt.mjs capture --url http://localhost:5206 --theme ledger --mode light --out snapshots/ledger-light
node util/visual-regression/vrt.mjs capture --url http://localhost:5206 --theme ledger --mode dark --out snapshots/ledger-dark
node util/visual-regression/vrt.mjs compare snapshots/base snapshots/head --out snapshots/diff
```

The default theme is Nova and the default mode is light. Pass `--browser google-chrome` when Chromium is installed under that name. Keep captures outside source control; the PR workflow produces its own base/head captures and report.

Run Ledger's focused interaction and style checks with `node util/visual-regression/verify-ledger.mjs http://localhost:5206`. Set `CHROME_PATH` to choose a browser binary. CI runs this check against the head app. It verifies slider keyboard operation, switch state, OTP entry, composite field focus, pressed/focused buttons, radius customization, reduced motion, and body/primary text contrast in both modes. It complements visual inspection; it is not a comprehensive accessibility audit.

Run Concourse’s focused checks with `node util/visual-regression/verify-concourse.mjs http://localhost:5206`. They exercise persistent selection versus hover, focused/pressed actions, read-only grouped validation, native toggle/slider/OTP input, sheet containment/dismissal, sidebar widths, and reduced motion in both modes at desktop/mobile widths.

Theme inventory and explicit component coverage are checked separately by `node util/theme-coverage/check.mjs`.

Run `node util/visual-regression/verify-native-select-popup.mjs http://localhost:5206` on a Hyprland desktop already using 1.6× display scaling to check Observatory and Concourse's native select popup borders. This opens an isolated temporary browser and uses `grim` to capture only that window: headless/CDP captures missed the desktop clipping. The check asserts the light popup's painted bottom edge and keyboard selection, and saves light/dark captures at two vertical positions under `snapshots/native-select-popup/`; inspect the dark captures visually. Load the specified webfonts, or set `OBSERVATORY_FONT_CSS` and `CONCOURSE_FONT_CSS` to local CSS files containing embedded font data for offline checks. Do not change desktop settings to run this check.

Run `node util/visual-regression/verify-concourse-progress.mjs http://localhost:5206` to check all four progress samples in both modes at desktop/mobile widths. Native scrollbars remain visible; the check catches fill overflow inside bordered tracks and verifies proportional fill widths, including empty/full progress. Captures are written to `snapshots/concourse-progress/`.
