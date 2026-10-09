# Theme checks

Checks for the token-driven stylesheet (`stellar-admin.css`, see [theme tokens](../../docs/design/tokens.md)). Each runs every page given in the base theme, each preset and seeded random themes, in light and dark mode, in headless Chromium over the DevTools protocol (`../visual-regression/browser.mjs`). Set `CHROME_PATH` or pass `--browser` when Chromium is installed under another name.

A theme is applied after the page loads: transitions off, the preset file's CSS appended, then the random knobs set on the root element. The mode is passed as `?mode=light|dark`, which both the prototype and DocsSamples read. Every third random theme is extreme: ranged knobs take one end of a wider range.

## Contrast

```bash
node util/theme-check/contrast.mjs "http://localhost:5206/Showcase/ThemeShowcase?theme=tokens" "http://localhost:5206/Table?theme=tokens"
node util/theme-check/contrast.mjs "http://127.0.0.1:8321/sandbox/html/token-spec/index.html?frame=1" "http://127.0.0.1:8321/sandbox/html/token-spec/index.html?frame=1&sample=index"
```

- Text: every element with its own visible text against its composited background, 4.5:1 or 3:1 for large text. Faded, hidden and disabled elements are skipped.
- Parts: every element with its own fill or border must separate from what is behind it by a luminance contrast of at least `--min-separation` (1.1), through its fill or its border, whichever is stronger; a real drop shadow also counts. A fill within 1.02 of its backdrop and no border is skipped as deliberately edgeless, the switch thumb is measured against its track, and a fill that fills a bordered parent shares its edge.

It prints a line per page, then the failing text and faint parts grouped by element. It exits with 1 when any text fails; faint parts are reported for review. Options: `--presets <folder>` (default `sandbox/html/token-spec/presets`, `none` for the base theme alone), `--random <count>` (12), `--seed <n>` (7), `--modes light,dark`, `--min-separation <ratio>`, `--out <report.json>`.

## Screenshots

```bash
node util/theme-check/screenshots.mjs --out util/theme-check/snapshots "http://localhost:5206/Showcase/ThemeShowcase?theme=tokens"
```

Writes `<theme>-<mode>-<page>.jpg` per page, full height up to `--max-height` (6000). Options: `--presets`, `--random` (6), `--seed`, `--modes`, `--width` (1400). Keep captures outside source control.
