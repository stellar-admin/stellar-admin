---
name: capture-scenario
description: Turn a plain-English description of what to show (a page, a state, an interaction) into a scenario (C#) and a spec (YAML) under tests/StellarAdmin.ScenarioTesting*, run it, and check the screenshots, annotated images, MP4 clips and GIFs it saves. Use when a developer asks for screenshots, a recording or an annotated image of the admin panel, or wants a scenario or spec written.
metadata:
  internal: true
---

# Capture a scenario

A **scenario** is a named admin panel configuration (C#). A **spec** is a YAML list of browser steps run against one scenario: it navigates, interacts, asserts, and saves PNGs and video clips. Tests and captures share the format but usually not the spec: a test spec asserts a lot, while a capture spec for the docs mostly sets the page up and saves its outputs. Read the [spec format reference](references/spec-format.md) before writing a spec.

Inspect Git status first and keep to the request: a capture does not authorize commits, library changes or website exports.

## Where things live

| What | Where |
| --- | --- |
| Scenarios | `tests/StellarAdmin.ScenarioTesting/Scenarios/<Name>Scenario.cs`, found by `ScenarioCatalog` |
| Scenario base class, host, database and sign-in | `tests/StellarAdmin.ScenarioTesting/Hosting/`; `ScenarioHost` adds the database and sign-in to every scenario |
| Voyager Travel model and seeder | `tests/VoyagerTravel/` (its own project), copied from the generator by `sync.sh`; don't edit |
| Sign-in | `Hosting/SignInSetup.cs`: `/scenario/sign-in` lists the users in the interactive host |
| Specs | `tests/StellarAdmin.ScenarioTesting.Tests/Specs/<name>.spec.yaml` (each file is also a test) |
| Spec runner and steps | `tests/StellarAdmin.ScenarioTesting/Runner/` (`SpecLoader.cs` holds every step) |
| Command line (interactive host and spec runs) | `tools/StellarAdmin.Cli`: `scenarios list`, `scenarios start` and `scenarios run` |

## Docs screenshots

Screenshots for the website's admin docs have their own scenarios and specs in `tools/StellarAdmin.Cli/Docs/Screenshots/`, in a folder per docs page (`Screenshots/Branding/`). `docs generate screenshots` uses only the scenarios in the CLI's own assembly, so a docs screenshot can't use a test scenario, and the test project doesn't run these specs. The `scenarios` commands see both. Static files a scenario links to, such as logos, go in `tools/StellarAdmin.Cli/wwwroot/`.

A docs page asks for a screenshot with a placeholder that the website renders as the image once it exists:

```mdx
<Screenshot name="branding-logo" alt="The top of the sidebar with each kind of logo" variants={["No logo", "Icon", "Wordmark"]} todo="Sidebar header crops of each kind of logo." />
```

The page shows `{name}.png`, or `{name}-{variant}.png` for each variant (the variant in lower case, with each run of other characters as `-`: `branding-logo-no-logo`), and `{name}-dark.png` in dark mode when it exists. `todo` is the brief; `alt` describes the image.

1. **List the open placeholders** from the repository root: `dotnet run --project tools/StellarAdmin.Cli -- docs check`. A placeholder is open while no spec has a `screenshot` step with its image's name; it lists the page and line, the missing names and the `todo`. A screenshot no page shows is reported as unused. Work only on the placeholders you were asked to capture.
2. **Write the scenario and spec** for each, as in the workflow below, with the screenshot names the placeholder expects and a `-dark` shot of each after `colorScheme: dark`. A scenario configures what the page's code example shows, with Voyager Travel branding. Where the seeded model makes that impossible, use a form model or other scenario-only types, or tell the developer the docs example has to change to match.
3. **Capture** with `dotnet run --project tools/StellarAdmin.Cli -- docs generate screenshots --spec <name>`, once per spec name (the file name without `.spec.yaml`), or without `--spec` for every spec. It writes the PNGs to the website's `public/images/admin/generated/` and rewrites `src/generated/screenshots.json` (each image's size, which the placeholder reads), then reports what is still open. A run without `--spec` deletes the folder first; a `--spec` run keeps the other screenshots. Check every image by eye: crops should end near their content (a `viewport` step shortens the window) and show the Voyager Travel title, not the default app name.
4. To change a screenshot, change its scenario or spec and capture again; the `todo` is not read once a spec exists. To rename one, rename both the screenshot and the placeholder, then capture without `--spec` so the old image goes.

`docs generate all` generates the demos too, and `docs clean screenshots` deletes the screenshots and the manifest.

## Workflow

1. **Pin down the capture.** From the description, decide: the admin configuration it needs, the pages and interactions, and the outputs (still PNG, element PNG, annotated PNG, MP4, GIF, dark mode, mobile width). Ask only when the description leaves a consequential choice open.
2. **Choose or add a scenario.** Reuse one in `Scenarios/` when its configuration fits. Otherwise add a `Scenario` subclass, following `SidebarBrandingTitleScenario`: a one-sentence `Description`, `ConfigureAdmin` with the configuration being shown and only the resources its specs use, and `StartPath` only if it isn't `/stellaradmin`. The class name gives the spec's `scenario` name in kebab case without "Scenario" (`SidebarBrandingTitleScenario` is `sidebar-branding-title`); a class with `[ScenarioArguments(...)]` attributes gives one scenario per attribute, with the values appended (`sidebar-branding-wordmark-layout-start`). Content uses the fictitious Voyager Travel theme.
3. **Check the data fits.** Every scenario runs on its own fresh in-memory copy of the seeded Voyager Travel database, so a spec can change data freely. Each scenario registers its own resources with `AddEfCoreResource<T>()` on the Voyager Travel model (trips, destinations, countries, bookings, travellers, guides and others). Call `index.EnablePaging()` on every resource's index: paging is off by default, and some tables have thousands of rows (Payments has about 13,000), which makes pages slow enough to time out on CI. Leave it off only when the spec is about an unpaged index, such as checking that every record shows. The seeded staff can sign in by email (`admin@voyager.travel` is Admin; `priya@`, `tomas@`, `hannah@`, `kenji@`, `chloe@` and `liam@voyager.travel` are Agents; `viewer@voyager.travel` is a Viewer); `authorization` requires an Admin or Agent for the panel and an Admin for Payments. If an existing scenario lacks a resource or column the capture needs, say so and ask before adding it.
4. **Find the locators.** Read the Razor views and tag helpers for the markup, or start the scenario and fetch its HTML: `dotnet run --project tools/StellarAdmin.Cli -- scenarios start <scenario> --urls http://127.0.0.1:5207` (`scenarios list` names them all). Never use port 5205. Stop the host when done.
5. **Write the spec.** One file per capture, named for what it shows. Start with a comment line saying what it shows, `goto` the page and set up the state to show. Add an `expect*` step only where the page must settle before a `screenshot` (after a navigation, or a dialog opening); the capture is not a test. Wrap interactions to record in `record`…`recordEnd` with `pause` steps so results stay on screen.
6. **Run it with outputs** from the repository root:

   ```sh
   dotnet run --project tools/StellarAdmin.Cli -- scenarios run tests/StellarAdmin.ScenarioTesting.Tests/Specs/<name>.spec.yaml --output artifacts/screenshots
   ```

   It prints `passed <name> (N steps)` or `failed <file>:<line>: <message>`. A failure names the spec line; fix the spec (usually a locator or a timeout) and rerun. `artifacts/` is ignored by Git.
7. **Check the output yourself.** Open every PNG it saved. For a clip, view frames rather than trusting the file exists: `ffmpeg -loglevel error -i <clip>.mp4 -vf "fps=2,scale=640:-1,tile=4x3" <scratchpad>/<clip>-sheet.png`, then open the sheet. Check the cursor reaches each target, dialogs and results are on screen long enough, and annotations point at the right elements. Adjust `pause`, `hold`, `cursorSpeed` and `typeDelay` and rerun until it reads well.
8. **Check the test run still passes.** The test project runs every spec in `Specs/`, captures included, without saving outputs: `dotnet run --project tests/StellarAdmin.ScenarioTesting.Tests`. All must pass. Screenshot, record and pause steps do nothing in this mode; the other steps still run.
9. **Report** the files added or changed and the output paths. Leave committing to the developer.

## Prerequisites

- Chromium: `/usr/bin/chromium`, or the path in `STELLARADMIN_CHROMIUM`.
- Recording needs Playwright's ffmpeg build. The runner's error names the install command (`pwsh bin/Debug/net10.0/playwright.ps1 install ffmpeg`, run in `tools/StellarAdmin.Cli` or the tests project).
- Cutting clips needs `ffmpeg` on the `PATH`.

## When a step is missing

If the description needs an interaction or assertion no step covers, add the step rather than working around it: a case in the `Step` switch in `SpecLoader.cs` (kept alphabetical) and a private method that parses the value and returns the step's `Func<SpecRun, Task>`. Pointer actions go through `Pointer(...)` so the cursor glides to them while recording. Errors throw `SpecException` with the spec's line. Add the step to the [spec format reference](references/spec-format.md) and verify it on a spec, with and without `--output`. Tell the developer you added it.

## Limits

- The video frame is fixed at 1280×800; a `viewport` step inside a recording scales the frame. Record at the default size.
- One clip at a time; clips cannot overlap or nest.
- Actions and assertions wait 1000 ms by default. A navigation or a slower state needs its own `timeout`.
