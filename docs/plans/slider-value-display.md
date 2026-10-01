# Slider value display and marks

Status: **active**. Phase 1 (design) approved 2026-10-01. Phase 2 (tag helpers and client) implemented and verified; awaiting review. Last updated: 2026-10-01.

`sa-slider` shows no current value, so a slider cannot be read precisely. This was recorded as a follow-up when the [field editor catalog](archive/field-editor-catalog.md) closed. Review widened the scope: since the slider already diverges from shadcn here, it should also support marks (ticks) and minimum and maximum labels, and stay composable in the shadcn style. The tag helper changes come first; `SliderEditor` exposes them afterwards. Work proceeds in phases with a review checkpoint after each; approval of one phase does not authorize the next.

| Phase | Scope | Status |
| --- | --- | --- |
| 1 | Design: anatomy, public API, formatting, accessibility | approved |
| 2 | Tag helper and client, TagHelpers tests, DocsSamples examples, theme coverage, regenerated skills reference, browser check across themes | implemented, awaiting review |
| 3 | Dashboard: `SliderEditor` settings, template, gallery, Dashboard reference and tests | not started |

## Survey

| Library | Value display | Marks | Other |
| --- | --- | --- | --- |
| shadcn (Base UI wrapper) | none | none | Root, Control, Track, Indicator, Thumb only. No theme styles a value or marks. |
| Base UI | `Slider.Value`, an `<output>` between label and control; `format` and `locale` via `Intl.NumberFormat`; render function receives all values | none | Examples put label and value in a header row above the track. |
| Ark UI (Zag) | `Slider.ValueText` part | `Slider.MarkerGroup` containing `Slider.Marker value={n}`; markers carry `data-value` and `data-state` (in or out of range) | `thumbAlignment` `center`/`contain`, `origin` start/center/end, thumb collision push/swap. |
| React Aria | `SliderOutput`, an `<output>`; `formatOptions` also drives each thumb's `aria-valuetext`; ranges show `30 – 60` | none | `thumbLabels` give each range thumb its own `aria-label`. |
| MUI | `valueLabelDisplay` off/auto/on: a bubble over each thumb | `marks` as `true` (every step) or `{ value, label }[]`; `step={null}` restricts to marks; `markActive` styling | Min and max labels demo: two captions under the track ends. `track` normal/inverted/none, `scale`. |
| Mantine | `label` bubble over the thumb, `labelAlwaysOn` | `marks` with `{ value, label }`; `restrictToMarks`; parts `markWrapper`, `mark`, `markLabel`, filled mark styling | `thumbLabel`, `thumbValueText`, `inverted`, `domain`, `scale`. |

The two composable libraries (Base UI, Ark) agree on the shape we need: a value part beside the label, and a mark group whose marks are positioned children of the control. The prop-driven libraries add bubbles over thumbs and restricting values to marks, which are deferred below.

APG's multi-thumb slider pattern requires each thumb to have its own accessible name and recommends `aria-valuetext` when the raw number is not user-friendly.

## Existing patterns in this repo

- `sa-progress` composes `<sa-progress-label>` and `<sa-progress-value>` children, and `<sa-progress-value/>` falls back to the computed value when empty. The slider value part follows it.
- `sa-input-otp` renders default slots when self-closing and the author's groups and slots when given children, sharing state through `InputOtpContext`. Marks follow it: `<sa-slider-marks/>` generates marks, or contains authored `<sa-slider-mark>` elements.
- `sa-marker` is an existing component, so the parts use "mark" rather than "marker".

## Current gaps found during the survey

- Thumbs have no accessible name. The field label targets the `sel-slider` host with `for`, but the host is not labelable and the thumbs have no `aria-labelledby`, so a screen reader announces an unnamed slider. A range slider additionally needs names that tell its thumbs apart.
- Range thumbs keep the overall `aria-valuemin` and `aria-valuemax`, whereas APG says dependent bounds update as the other thumb moves.

## Proposed anatomy

The slider does not own its label. Like every field input it sits inside `<sa-field>` beside `<sa-field-label>`, `<sa-field-description>` and `<sa-field-error>`; the `label`/`asp-for` attributes are only a shortcut that builds that field automatically. The value part therefore stands alone and the author places it, and marks are children of the slider because they belong to the control:

```razor
<sa-field>
    <div class="flex items-center justify-between gap-2">
        <sa-field-label asp-for="PricePerNight"/>
        <sa-slider-value/>
    </div>
    <sa-slider asp-for="PricePerNight" max="1000" step="50" value-format="${0}">
        <sa-slider-marks interval="250" labels="SliderMarkLabels.Ends"/>
    </sa-slider>
    <sa-field-description>Nightly rate before taxes.</sa-field-description>
</sa-field>

<sa-slider id="rating" value="4" min="1" max="5">
    <sa-slider-marks>
        <sa-slider-mark value="1">Poor</sa-slider-mark>
        <sa-slider-mark value="3">Good</sa-slider-mark>
        <sa-slider-mark value="5">Excellent</sa-slider-mark>
    </sa-slider-marks>
</sa-slider>
```

A self-closing `<sa-slider/>` renders exactly as today. `sa-slider` drops `TagStructure.WithoutEndTag` so it accepts mark children, and publishes a `SliderContext` (bounds, values, orientation, thumb alignment, format, culture, class names) before rendering them. The label row in the example is the author's own `div`, as in shadcn's Field examples; it is not a part.

### `sa-slider` (changed)

| Attribute | Type | Default | Meaning |
| --- | --- | --- | --- |
| `value-format` | `string` | — | A template with a `{0}` placeholder applied to every displayed number (value part, generated mark labels, `aria-valuetext`), e.g. `"${0}"` or `"{0} km"`. |
| `thumb-labels` | `string` | — | Comma-separated accessible names for the thumbs of a range slider, e.g. `"Minimum price,Maximum price"`. Single-thumb sliders are named by the field label. |

`SliderClassNames` gains `Marks`, `Mark` and `MarkLabel`. The value part takes its own `class`.

### `<sa-slider-value>`

| Attribute | Type | Meaning |
| --- | --- | --- |
| `for` | `string?` | The `id` of the slider whose value it shows. Needed only outside a field, or when one field holds two sliders. |
| `index` | `int?` | Shows one thumb's value; without it, all values. |

Renders `<output data-slot="slider-value" aria-live="off">`, with `for` when given. Inside `<sa-field>` it needs no attributes: it shows the value of the slider in the same field, wherever the author places it (beside the label, inside the description as shadcn's Field demo does for a price range, under each end of the track or after it). Outside a field, `for` names the slider, which then needs an explicit `id`. There is no `asp-for`: pairing through the field covers bound sliders without knowing the generated id. Without `index`, two values join with a spaced en dash (`200 – 800`) and three or more with commas. It takes no content, because the client rewrites its text on every change.

The value part usually comes before the slider in the document, so the server cannot know the slider's values when it renders, and the output starts empty. `sel-slider` fills every output linked to it when it hydrates, at the same moment the slider becomes interactive. Sitting beside a label, the empty output causes no layout shift. A server-rendered initial text is possible with deferred content written after the page has executed, but it breaks under view flushing and is not proposed.

### `<sa-slider-marks>`

Renders a `<span data-slot="slider-marks">` positioned over the control, without intercepting pointer events. Children are authored `<sa-slider-mark>` elements. When it has none, it generates marks:

| Attribute | Type | Default | Meaning |
| --- | --- | --- | --- |
| `interval` | `int?` | `step` | Generates a mark every `interval` from `min`, plus one at `max`. Throws when it would generate more than 100 marks. |
| `labels` | `SliderMarkLabels?` | `None` | Which generated marks show their formatted value: `None`, `Ends` (minimum and maximum) or `All`. |
| `show-ticks` | `bool?` | `true` | Whether marks draw a tick. `false` with `labels="SliderMarkLabels.Ends"` is the plain minimum and maximum caption pattern. |

`SliderMarkLabels` is a new enum, dedicated to this component.

### `<sa-slider-mark>`

| Attribute | Type | Meaning |
| --- | --- | --- |
| `value` | `int` (required) | Position of the mark. Throws when outside `min`..`max`. |

Renders `<span data-slot="slider-mark" data-value="25" data-state="in-range|out-of-range">` with a tick and, when it has content, a `<span data-slot="slider-mark-label">` holding that content. `data-state` follows Ark: a mark inside the filled range is `in-range`, so themes can colour passed ticks. The server sets it and the client updates it while dragging.

## Layout

- Placing the value is the author's layout, so the slider, the field base class and the field wrapper are unchanged by it.
- The `label`/`asp-for` shortcut builds the field itself and stays unchanged, so it has nowhere to place a value. To show one, the author writes the field explicitly: `<sa-field-label asp-for>`, `<sa-field-description asp-for>` and `<sa-field-error asp-for>` already exist, and a slider inside `<sa-field>` already skips its own wrapper, so no `render-field="false"` is needed.
- In the explicit form the slider does not link its thumbs to the description and error, because `ApplyFieldAttributes` only does so when the slider renders the field itself. This gap affects every input in explicit mode. An author can close it by giving the description and error ids and setting `aria-describedby` on `<sa-slider>`, which the slider already moves onto its thumbs; the Dashboard template does this. A field-level fix (the explicit field registering its parts' ids for the input) is a follow-up outside this plan.
- Marks sit on the track; labels sit below a horizontal track and beside a vertical one. A structural rule reserves room for labels on the host (`has-[[data-slot=slider-mark-label]]`), so labels never overlap the description.
- Marks must line up with the thumb centres. With centre alignment a mark sits at its percentage. With edge alignment the thumb centre moves inward by up to half a thumb at the ends, so marks use `calc()` with `--sa-slider-thumb-size`. The client measures the thumb and sets that property on hydration; before hydration the end marks can be up to half a thumb out. Phase 2 checks whether themes should declare the property to remove that shift.
- The value is outside the host, so a disabled slider's `opacity-50` does not dim it. Read-only Dashboard sliders render disabled, and this keeps their value legible.
- The value's default style must read well wherever it is placed. Phase 2 decides between inheriting the surrounding text (fitting inside a description) and a muted `text-sm` (fitting beside a label); the author's `class` overrides either.

## Formatting

Values are integers. `value-format` lives on the slider, because the thumbs' `aria-valuetext` and generated mark labels need it too, and every value part linked to the slider uses it. The server formats each number with `CultureInfo.CurrentCulture` and no decimals, then substitutes it into `value-format`. The host carries `data-value-locale` and `data-value-format` so the client formats the same way with `Intl.NumberFormat(locale, { maximumFractionDigits: 0 })`. A `value-format` without `{0}` throws. The value uses `tabular-nums`. `[DisplayFormat(DataFormatString)]` is not used: its .NET composite formats cannot be reproduced on the client.

## Client behaviour

`sel-slider` finds its value parts in two ways: outputs without `for` inside its closest `[data-slot="field"]`, and `output[data-slot="slider-value"][for="<host id>"]` in its root node when the host has an id. Its marks are within the host. It fills the outputs on hydration. `#setThumbValue` already runs for drags, track clicks and keys; it gains updates to the outputs, each thumb's `aria-valuetext`, and mark `data-state`. The server renders `aria-valuetext` and mark state initially.

## Accessibility

- Thumbs get names (in scope for Phase 2): a single thumb gets `aria-labelledby` pointing at the field's label, the `[data-slot="field-label"]` in its closest field, or else a label whose `for` matches the host id; range thumbs get `aria-label` from `thumb-labels`, falling back to that label for every thumb when absent. In the explicit form the label is the author's `<sa-field-label>`, which the server cannot see from the slider, so the client sets `aria-labelledby` on hydration (minting the label an id if it has none); the shortcut form renders it on the server. This fixes the existing gap.
- With `value-format`, each thumb gets `aria-valuetext` (`$500`), kept in sync by the client.
- The `<output>` has `aria-live="off"`: its implicit status role would otherwise announce every drag step, duplicating the thumb.
- Marks and their labels are `aria-hidden`: they repeat information the thumbs expose and would otherwise be read as stray text.

## Theme coverage

New hooks: `sa-slider-value`, `sa-slider-marks`, `sa-slider-mark` and `sa-slider-mark-label`. Structural rules go in `components.css`. Each theme needs a reviewed decision for tick size and colour (including `in-range`), label typography and value typography; the shadcn themes share one treatment derived from their muted-foreground and border tokens, and the custom themes follow their specifications (for example a mono face for numbers where the spec uses one). The coverage manifest records each decision.

## Deferred

These appear in other libraries but are not proposed now; each can be added later without changing the parts above.

- A value bubble over each thumb (MUI `valueLabelDisplay`, Mantine `label`).
- Restricting values to marks (MUI `step={null}`, Mantine `restrictToMarks`).
- Clicking a mark label to move the nearest thumb there.
- Dependent `aria-valuemin` and `aria-valuemax` on range thumbs.
- Inverted tracks, non-linear scales, origins other than the start.

## Dashboard (Phase 3 outline)

`SliderEditor` gains `ShowValue` (`bool`, default `true`, configurable per editor), `ValueFormat` (`string?`), `MarkInterval` (`int?`) and `MarkLabels` (`SliderMarkLabels?`). The `Editors/Slider` template composes the explicit field on the user's behalf: `<sa-field>` with `<sa-field-label asp-for>` and `<sa-slider-value/>` in a header row, the slider, and the description and error parts. It gives the description and error ids and passes them to the slider as `aria-describedby`, so the thumbs keep the descriptions the existing HTTP tests check.

## Decisions

Recorded 2026-10-01 after review.

1. Thumb names and `thumb-labels` are in scope for Phase 2.
2. The `label`/`asp-for` shortcut stays unchanged; showing a value requires the explicit field. Bound sliders work there with `asp-for`, and the Dashboard template composes it for the user.
3. `<sa-slider-marks>` keeps generation (`interval`, `labels`, `show-ticks`) and accepts authored `<sa-slider-mark>` children, which replace generation.
4. `SliderEditor.ShowValue` defaults to `true` and can be turned off per editor.
5. The explicit-field `aria-describedby` gap is handled in the Dashboard template; the field-level fix is a follow-up.
6. `<sa-slider-value>` pairs with the slider in its field and takes `for` only outside one; no `asp-for`, and no generated id on the bound host.
7. Visual defaults from the prototype: value beside the label, muted `text-sm` with `tabular-nums` and a spaced en dash, notch ticks with `in-range` state, and end labels centred under their marks (later changed to align with the track's ends; see Phase 2 implementation). Each theme's coverage decision starts from these.

## Phase 2 implementation

The tag helpers live in `src/StellarAdmin.TagHelpers/TagHelpers/Slider/`: `SliderValueTagHelper`, `SliderMarksTagHelper`, `SliderMarkTagHelper`, `SliderMarkLabels`, the internal `SliderContext`, `SliderMarksContext` and `SliderMarkRenderer`, and the changes to `SliderTagHelper` and `SliderClassNames`. `sel-slider.ts` gained the client behaviour. Structural rules are in `components.css`; themed rules are in each `Themes/<Theme>.custom.css` (copied into the matching generated `shadcn.*.css` as the generator would) and in a closing block of each hand-authored theme. DocsSamples gained the Value, Marks and Custom Marks examples, and the skills manifest surfaces Value and Custom Marks.

Differences from the design, for review:

- Each mark renders its tick as a `<span data-slot="slider-mark-tick" class="sa-slider-mark-tick">`, so `show-ticks="false"` can drop the tick and keep the label. This adds the hook `sa-slider-mark-tick`; it has no `SliderClassNames` entry.
- Generation throws above 200 marks rather than 100, because a 0–100 slider at the default step of 1 needs 101. With `show-ticks="false"`, only labelled marks are generated, so `labels="SliderMarkLabels.Ends"` renders two marks whatever the interval.
- Themes do not declare `--sa-slider-thumb-size`. The client measures the first thumb with a `ResizeObserver`, which also covers sliders shown later (tabs, dialogs); a one-off measurement on connection sometimes ran before the theme stylesheet applied. Before hydration the structural rules fall back to `1rem`, so edge-aligned end marks can be off by at most a pixel or two.
- Mark labels and the room reserved for them are positioned from the thumb size: below a horizontal track by half a thumb plus `0.25rem`, and the host gains a matching bottom margin (`calc(thumb / 2 + 1.25rem)`); a vertical slider reserves `calc(thumb / 2 + 3rem)` at its inline end.
- In the shortcut form the field label now gets an id and no `for`, and the thumbs reference it with `aria-labelledby` (the existing `LabelId` mechanism used by choice groups). An author's `aria-labelledby` on `<sa-slider>` moves to the thumbs, like `aria-describedby`.
- The host always carries `data-value-locale` when the current culture has a name.

Visual points found in the browser check, then changed after review:

- Centred end labels extended half their width beyond the track; at 390 pixels the Custom Marks example's "Excellent" crossed its card's padding and border. Marks at the minimum and maximum now carry `data-bound="min|max"`, and their labels align to the track's ends (start-aligned at the minimum, end-aligned at the maximum, and above or below the end mark when vertical). Edge-aligned thumbs inset the end marks by half a thumb, so those labels shift by half a thumb to sit flush with the track. Labels between the ends stay centred.
- The custom themes style the value like the progress value (mono, 12 pixels, muted), which read smaller than the surrounding text inside a sentence. A structural rule now makes a value inside a `p` or field description inherit the text's font family, size, line height and letter spacing in every theme; it keeps the theme's muted colour and `tabular-nums`. The value beside a label keeps the themed style.

## Prototype

`sandbox/html/slider.html` shows the candidate visuals against the real theme bundles and the real `sel-slider`, with the Razor for each composition: the value composed beside the label, inside the description, under each end, after the track and outside a field, the unchanged label shortcut, the deferred thumb bubble, value typography and range separator, three tick treatments with and without in-range state, mark labels (Ends centred or flush, All, minimum and maximum captions without ticks), authored and uneven marks, edge versus centre alignment with a pre-hydration toggle, vertical sliders, and disabled sliders. A small page script stands in for the proposed client updates.

## Verification

No product code has changed. The prototype was captured in headless Chromium at 1280 pixels in shadcn Nova, Observatory and shadcn Vega dark, and at 390 pixels in Parallax, with no script errors. Five ArrowRight presses on the candidate's upper price thumb changed the value text from `$200 – $800` to `$200 – $1,000`, set `aria-valuetext` to `$1,000` and marked the 1,000 tick in range. After the value part became standalone, the outputs filled on hydration, and two ArrowRight presses on the lower thumb of the description composition changed its indexed outputs from `$200`/`$800` to `$300`/`$800`.

Phase 2, recorded 2026-10-01:

- `dotnet test tests/StellarAdmin.TagHelpers.Tests`: 213 passed, including new tests for value formatting and culture, the placeholder check, thumb naming by label id and by `thumb-labels`, mark order within the host, generation by interval and step, Ends labels with a format, tickless generation, the mark limit, range state, centre and edge positions, authored marks replacing generation, mark bounds, mark class names, the value part's attributes, and an author's `id` kept on a bound host.
- `dotnet test tests/StellarAdmin.TagHelpers.IntegrationTests`: 19 passed. `dotnet test tests/StellarAdmin.Dashboard.IntegrationTests`: 241 passed.
- `node util/theme-coverage/check.mjs`: 58 components × 15 themes reviewed. `npm run build` in the client compiled every bundle with the new rules.
- `dotnet run --project util/SkillsGenerator` regenerated `slider.md` and the components index; `-- --check` reports no drift.
- DocsSamples on port 5206 in headless Chromium, with no script errors: the Value example filled `$200 – $800` and `15 km` on hydration, named the range thumbs from `thumb-labels` and the distance thumb from the explicit field label. Four ArrowRight presses on the upper price thumb and one ArrowLeft on the lower gave `$150 – $1,000`, matching `aria-valuetext` and hidden inputs `150` and `1000`; End on the distance thumb gave `50 km`. Twelve ArrowRight presses on the Marks example's upper thumb moved the 1,000 mark into range. Marks were captured in shadcn Nova (light and dark), Observatory (light and dark), shadcn Luma, Aurora and Parallax, and the full page at 390 pixels in Parallax dark without horizontal overflow; thumb sizes measured 12, 15 and 24 pixels as themed. A slider converted to vertical in the page placed the 4 tick at the thumb's centre (164.58 pixels against 164.58).
- Review follow-up, recorded 2026-10-02 (flush end labels and value typography in prose): `dotnet test tests/StellarAdmin.TagHelpers.Tests` 214 passed, adding a test for `data-bound` on the minimum and maximum marks; `npm run build` compiled every bundle; the coverage check reviewed 58 components × 15 themes. DocsSamples on port 5206 at 390 pixels: Custom Marks and Marks in Observatory show "Poor"/"$0"/"1"/"0 km" starting at the track's start and "Excellent"/"$1,000"/"8"/"100 km" ending at its end without horizontal overflow; the Value example in Aurora shows `15 km` in the description's sans text, and Custom Marks' sentence value matches its text size, while the header value keeps the mono style. shadcn Nova Value and Custom Marks were also captured.
- Not checked: a real vertical slider with marks rendered by the server (no example uses one; this includes the vertical end-label alignment), right-to-left layout, and mark positions in the Dashboard (Phase 3).

