# Spec format

A spec is a `.spec.yaml` file with a `scenario` and a list of `steps`. Each step is a single `kind: value` pair. The runner starts the scenario on a free loopback port, opens a fresh 1280×800 Chromium context (light scheme, reduced motion) and runs the steps in order. The first failing step stops the run with the file and line.

```yaml
# The sidebar link opens the trips index with its rows
scenario: default
steps:
  - goto: /stellaradmin
  - click: { role: link, name: Trips }
  - expectUrl: /stellaradmin/trips
  - expectVisible: { role: cell, name: Kyoto in autumn }
  - screenshot: trips-index
```

Outputs are saved only when the run has a screenshot folder (`--screenshots <dir>` on the host, or `STELLARADMIN_SCREENSHOTS=1` for the tests). Without one, `screenshot`, `record`, `recordEnd` and `pause` do nothing and every other step still runs.

## Locators

Steps that take an element take a locator:

| Form | Meaning |
| --- | --- |
| `".sa-card-title"` | A CSS selector (Playwright selectors such as `:has()` and `:has-text()` work). |
| `{ selector: ".sa-card-title" }` | The same, as a mapping. |
| `{ role: heading }` | The ARIA role. |
| `{ role: link, name: Trips }` | The role with this exact accessible name. |

Prefer roles and names, which read like the page. A table row's accessible name is all its cells joined, so select a row with `"tr:has-text('Kyoto in autumn')"`, not by role. A locator that matches several elements fails an action; narrow it.

Any step whose value is a mapping can add `timeout` in milliseconds (default 1000): `{ role: dialog, timeout: 3000 }`.

## Navigation and assertions

| Step | Value |
| --- | --- |
| `goto` | A path, relative to the scenario's base URL. Fails unless the response is OK. |
| `expectUrl` | The end of the URL, e.g. `/stellaradmin/trips`, or `{ url, timeout }`. |
| `expectVisible` | A locator. |
| `expectText` | A locator mapping plus the exact `text`: `{ role: heading, name: Trips, text: Trips }` or `{ selector: ".sa-sidebar-brand-title", text: Voyager Travel }`. |

Assertions retry until the timeout, so they also wait for a state to settle.

## Interactions

| Step | Value |
| --- | --- |
| `click` | A locator. |
| `hover` | A locator. |
| `check` / `uncheck` | A checkbox or radio locator. |
| `fill` | `{ locator, text }`: replaces the field's value at once. |
| `type` | `{ locator, text }`: types key by key (at the clip's `typeDelay` while recording). |
| `select` | `{ locator, option }`: picks a `<select>` option by value or label. |
| `press` | A key or chord on the focused element: `Enter`, `Escape`, `Control+K`. |
| `scroll` | A locator to scroll into view (smoothly while recording). |

```yaml
  - click: "button[commandfor='--command-palette']"
  - fill: { locator: "#--command-palette input", text: Trips }
  - press: Enter
  # On a form (the trips fixture has none)
  - select: { locator: { role: combobox, name: Country }, option: Japan }
  - check: { role: checkbox, name: Featured }
```

## Page state

| Step | Value |
| --- | --- |
| `colorScheme` | `dark` or `light`; switches the page's class without reloading. |
| `viewport` | `{ width, height }` in pixels; resizes without reloading. |

## Screenshots

| Step | Value |
| --- | --- |
| `screenshot` | A name: saves the viewport as `{name}.png`. |
| `screenshot` | `{ name, locator }`: saves only that element. |

```yaml
  - screenshot: branding
  - screenshot: { name: branding-brand, locator: "a:has(.sa-sidebar-brand-mark)" }
  - colorScheme: dark
  - screenshot: branding-dark
  - viewport: { width: 390, height: 844 }
  - screenshot: branding-mobile-dark
```

## Annotations

Annotations draw over the page and stay until `clearAnnotations: true`, so every screenshot after them includes them.

`annotate` takes `{ locator, style, badge, label, placement, arrow, gap }`:

| Key | Values |
| --- | --- |
| `locator` | Required; the element to mark. |
| `style` | `box` (default), `highlight`, `circle`, `underline`, or `none` (label or badge only). |
| `badge` | Text for a round badge on the element (usually a number), or `auto` for the next number after the badges already shown. |
| `label` | A text callout beside the element. |
| `placement` | Where the label goes: `top`, `right`, `bottom` or `left`. Without one it goes below, or above when there's no room. |
| `arrow` | `true` draws an arrow from the label to the element. |
| `gap` | Pixels between the element and a label with an arrow (default 56). |

`spotlight` takes a locator and dims the page except for every element it matches; annotations still draw on top.

```yaml
  - annotate: { locator: ".sa-sidebar-brand-mark", style: circle, label: Icon logo, placement: bottom, arrow: true }
  - annotate: { locator: ".sa-sidebar-brand-text", label: Title and subtitle, placement: right, arrow: true }
  - screenshot: branding-annotated
  - clearAnnotations: true
  - spotlight: "a:has(.sa-sidebar-brand-mark)"
  - annotate: { locator: "a:has(.sa-sidebar-brand-mark)", style: none, label: Set with ConfigureBranding, placement: right, arrow: true }
  - screenshot: branding-spotlight
  - clearAnnotations: true
  - annotate: { locator: ".sa-page-header-title", badge: auto }
  - annotate: { locator: ".sa-card:has(.sa-card-title)", badge: auto }
  - screenshot: branding-numbered
```

## Recordings

A spec with a `record` step records its whole run when outputs are saved; each `record`…`recordEnd` stretch is cut out as `{name}.mp4`. Inside a clip, motion is switched on, a drawn cursor glides to each element before `click`, `hover`, `check`, `uncheck`, `fill` and `select`, and presses show a ripple. A clip still open at the end of the spec ends there. Clips cannot overlap.

| Step | Value |
| --- | --- |
| `record` | A clip name, or `{ name, gif, typeDelay, cursorSpeed, hold }`. |
| `recordEnd` | `true`. |
| `pause` | Milliseconds to wait while recording, so a result stays on screen. |

| `record` option | Default | Meaning |
| --- | --- | --- |
| `gif` | `false` | `true` also saves `{name}.gif`. |
| `typeDelay` | `90` | Milliseconds per key for `type`. |
| `cursorSpeed` | `1` | Scales the cursor's glide; `2` is twice as fast. |
| `hold` | `800` | Milliseconds the last frame stays before the clip ends. |

```yaml
# The command palette finds the trips page
scenario: default
steps:
  - goto: /stellaradmin
  - record: { name: command-palette, gif: true }
  - pause: 400
  - click: "button[commandfor='--command-palette']"
  - pause: 300
  - type: { locator: "#--command-palette input", text: Trips }
  - pause: 500
  - press: Enter
  - expectUrl: { url: /stellaradmin/trips, timeout: 3000 }
  - pause: 600
  - click: { role: cell, name: Kyoto in autumn }
  - pause: 800
  - recordEnd: true
```

Pace a clip with `pause` after each visible change (300–800 ms reads well), and start it after any setup that shouldn't be shown: navigate first, then `record`.
