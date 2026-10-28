---
name: write-docs-screenshot-specs
description: Write the scenarios and specs for the <Screenshot> placeholders on the website's admin docs pages that have no images yet, generate the screenshots and GIFs, and check them. Use when a developer asks to capture, fill or generate docs screenshots or open placeholders, or to change or rename a docs screenshot.
metadata:
  internal: true
---

# Write docs screenshot specs

A docs page asks for a screenshot with a `<Screenshot>` placeholder. This skill writes the scenario (C#) and spec (YAML) that produce each open placeholder's images, generates them into the website and checks them. For how to write a scenario and a spec, run one and check its output, follow [write-scenario-specs](../write-scenario-specs/SKILL.md) and its [spec format reference](../write-scenario-specs/references/spec-format.md).

Inspect Git status in the product and the website first, and keep to the request: a capture does not authorize commits or library changes. Work only on the placeholders you were asked to capture.

## Placeholders

```mdx
<Screenshot name="branding-logo" alt="The top of the sidebar with each kind of logo" variants={["No logo", "Icon", "Wordmark"]} todo="Sidebar header crops of each kind of logo." />
```

The page shows `{name}.png`, or `{name}-{variant}.png` for each variant (the variant in lower case, with each run of other characters as `-`: `branding-logo-no-logo`), and `{name}-dark.png` in dark mode when it exists. A GIF shows as `{name}.gif`. Until a spec saves the image, the page shows the placeholder with its `todo`, the brief; `alt` describes the image. The `todo` is not read once a spec exists.

## Names and folders

Docs scenarios and specs live in `tools/StellarAdmin.Cli/Docs/Screenshots/`, in a folder per docs page, named after the page's file (`SortingAndPaging/` for `sorting-and-paging.mdx`). A scenario, its spec and its screenshots share one name, `<page>-<what>`: `ScopesTabsScenario` is `scopes-tabs`, run by `scopes-tabs.spec.yaml`, which saves `scopes-tabs.png`. Each spec has its own scenario; two specs never share one, because a change for one spec can silently break the other. A spec can save more than one screenshot from its scenario, each with its own `<page>-` name. A screenshot shown on several pages is named for the page it mainly belongs to. `docs check` fails a scenario, spec or screenshot that doesn't start with its folder's page, and a `<Screenshot>` whose images don't start with a page that shows it.

`docs generate screenshots` uses only the scenarios in the CLI's own assembly, so a docs screenshot can't use a test scenario, and the test project doesn't run these specs. The `scenarios` commands see both. Pages, controllers and static files a scenario links to, such as `/Admin/Departures` or logos, go in the host app, `tests/StellarAdmin.ScenarioTesting.Host/`. Every scenario shares it. Helpers several scenarios use, such as `Travellers.cs`, stay in the `Screenshots/` root.

## Workflow

1. **List the open placeholders** from the repository root: `dotnet run --project tools/StellarAdmin.Cli -- docs check`. A placeholder is open while no spec saves its image's name; it lists the page and line, the missing names and the `todo`. A screenshot no page shows is reported as unused, and a misnamed scenario, spec or screenshot as misnamed.
2. **Write the scenario and spec** for each, with the screenshot names the placeholder expects and a `-dark` shot of each after `colorScheme: dark`. For a GIF, the spec records a clip with the placeholder's name and `gif: true` instead, and a second clip named `-dark` after `colorScheme: dark`; the generator deletes the clips' MP4s. A scenario configures what the page's code example shows, with Voyager Travel branding. Where the seeded model makes that impossible, use a form model or other scenario-only types, or change the docs example to the seed's names where the screenshot shows it. Use a `margin` on an element screenshot so it shows a little of the page around the element, and a `viewport` step to shorten window shots.
3. **Capture** with `dotnet run --project tools/StellarAdmin.Cli -- docs generate screenshots --spec <name>`, once per spec name (the file name without `.spec.yaml`), or without `--spec` for every spec. It writes the images to the website's `public/images/admin/generated/` and rewrites `src/generated/screenshots.json` (each image's size, and `format: "gif"` for a GIF, which the placeholder reads), then reports what is still open, unused or misnamed. A run without `--spec` deletes the folder first; a `--spec` run keeps the other screenshots.
4. **Check every image by eye**, light and dark: crops should end near their content, show the Voyager Travel title rather than the default app name, and annotations should point at the right elements. Check a GIF as frame sheets, as for a clip in `write-scenario-specs`.
5. **Report** the placeholders captured, the files added or changed in each repository, and any docs example you changed. Leave committing to the developer.

To change a screenshot, change its scenario or spec and capture again. To rename one, rename the scenario, spec, screenshot and placeholder together, then capture without `--spec` so the old image goes.

`docs generate all` generates the demos too, and `docs clean screenshots` deletes the screenshots and the manifest.
