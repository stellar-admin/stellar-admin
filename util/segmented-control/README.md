# Segmented control verification

Run DocsSamples on an available port, then run the browser checks using Node and system Chromium:

```bash
node util/segmented-control/check.mjs http://localhost:5206
```

The script compares the segmented control with default tabs in all nine themes, light/dark modes, and desktop/mobile widths. It also exercises automatic field labels, metadata descriptions, validation messages, labels, independent groups, disabled options, keyboard focus and navigation, change-event counts, reset, and successful/invalid form submissions with a prefixed nullable enum property. It starts and stops its own Chromium process; the samples app remains running.

Screenshots default to `/tmp/stellar-admin-segmented-control`; supply a second argument to choose another output directory. Set `CHROME_PATH` to override the Chromium executable. The samples app requires its usual font network access for visual checks.
