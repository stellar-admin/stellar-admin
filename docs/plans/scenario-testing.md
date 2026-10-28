# Scenario testing

Status: in progress (2026-10-27), on branch `scenario-testing-poc`.

Browser tests of the admin panel driven by **scenarios** (a named admin panel configuration in C#) and **specs** (YAML step lists run against one scenario). The same runner saves screenshots, annotated images and video clips, but the focus is tests; docs captures will probably move into the DocsSamples generator later, so test and capture specs are not split now.

## Built

- `tests/StellarAdmin.ScenarioTesting`: `Hosting/` (`Scenario`, `ScenarioHost`, and the database and sign-in every scenario gets; the browser tests and the interactive host share one app), `Scenarios/` (`ScenarioCatalog` and the scenarios), and the spec runner in `Runner/`.
- Spec steps: `goto`, `expectUrl`, `expectVisible`, `expectText`, `click`, `hover`, `check`, `uncheck`, `fill`, `type`, `select`, `press`, `scroll`, `colorScheme`, `viewport`, `screenshot` (viewport or element), `annotate`, `clearAnnotations`, `spotlight`, `record`, `recordEnd` and `pause`.
- Recording: the whole run is recorded and each `record`…`recordEnd` stretch is cut into an MP4 (and an optional GIF) with a drawn cursor, ripples and pacing options.
- `tests/StellarAdmin.ScenarioTesting.Tests` runs every spec in `Specs/` as a test; outputs save only with `STELLARADMIN_SCREENSHOTS=1`.
- `tools/StellarAdmin.Cli` `scenarios` commands: `list`, `start <scenario> [--urls <url>]` hosts one interactively, and `run <file|dir>... [--output <dir>]` runs specs.
- The `capture-scenario` development skill writes, runs and checks a scenario and spec from a plain-English description.
- Layout (2026-10-27): `tests/VoyagerTravel` holds the copied model; the runner folder is `Runner/` because a `SpecRunner` namespace would hide the `SpecRunner` class. The TestServer branding test was removed: `branding.spec.yaml` checks the same title, subtitle and icon.

## Verified

- The three specs (`trips-index`, `branding`, `command-palette`) pass in the test project and through the host, with and without outputs.
- Screenshots, annotated images and clips were checked by eye; the skill's run command was checked on `trips-index`.
- Not checked: Playwright's own Chromium (only the system `/usr/bin/chromium`), CI, Windows or macOS.

## Test data

Built (2026-10-27): every scenario runs on the Voyager Travel database, on EF Core with SQLite. The read-only trips fixture was removed. Each scenario registers only the resources its specs use: `default` has trips with search, create, edit and delete; `branding` and `staff-only` a trips index. Without a Destination resource, the trip form's `DestinationId` is a plain number input.

- `tests/VoyagerTravel` is its own project holding the model, `ApplicationDbContext` and seeder from `voyager-travel-generator`, copied as-is by `sync.sh` (the generator stays the single source). The only change is the seed's anchor day, fixed at 2026-10-18 instead of the day it runs, so dates don't move between runs.
- `Hosting/DatabaseSetup.cs`: the template is created and seeded once per process in a private in-memory database (about 7 s) and never written. Each app gets its own copy through SQLite's backup API (a few milliseconds) in a named shared-cache in-memory database, held open until the app's services are disposed. `ScenarioHost` adds it to every scenario. Every spec run starts from the same data, and parallel runs share nothing.
- Specs: `trip-edit` changes a trip's title; `trips-index` checks the seeded title is still there.
- Packages added: Bogus 35.6.5, Microsoft.AspNetCore.Identity.EntityFrameworkCore 10.0.11, and Microsoft.EntityFrameworkCore.Sqlite in the scenarios project.

## Login

Built (2026-10-27): ASP.NET Core Identity on the seeded users, added to every scenario by `ScenarioHost` (`Hosting/SignInSetup.cs`).

- `/scenario/sign-in` lists the users and signs one in by email with Identity's cookie, no password; it is Identity's login path, and `/scenario/access-denied` its access-denied path. Log out in the user menu signs out of Identity's cookie.
- Specs sign in with a top-level `user: <email>` key, or a `signIn: <email>` step to switch users partway.
- The `staff-only` scenario requires the Admin or Agent role. Specs: `sign-in` (redirect to sign-in and back), `sign-out` (Log out in the user menu), `access-denied` (a Viewer is refused, then an Agent signs in).
- The user menu shows the email as the name: Identity's name claim is the user name, which the seed sets to the email, and `DisplayName` isn't a claim.

## Verified

- All seven specs pass in the test project (8 of 8 tests with the TestServer branding test, run in parallel) and through the host. `trips-index` and the `command-palette` clip were checked by eye on the new data.
- After the layout change: the seven specs pass in the test project and the Cli project builds. Not re-checked: spec runs through the Cli, and outputs by eye.

## Pilot: integration tests as scenarios

Built (2026-10-27): the six tests in `tests/StellarAdmin.IntegrationTests/Sidebar/SidebarBrandingTests.cs` (seven cases) as one scenario and spec each, in `Scenarios/Sidebar/` and `Specs/Sidebar/`. New steps `expectAttribute`, `expectCount` and `expectTitle`, and `/pattern/` values in text steps. The Start and Fill layout rows are two instances of one scenario class that takes the layout. `SidebarBrandingTests.cs` was deleted; `sidebar-branding-title` checks the home page, whose browser title is the branding title alone (the old test used a resource page, "Products - …").

- The default title comes from the entry assembly's name, so it differs between the test project and the Cli; `sidebar-branding-none` matches `/^Stellar Admin Scenario Testing /`. Pinning `ApplicationName` doesn't work: MVC loads the assembly by that name.
- Image and wordmark paths don't exist; the specs check attributes only.
- Verified: 14 of 14 tests pass in the test project, and the seven sidebar specs pass through the Cli. A wrong attribute, count, title, pattern and meta value each failed with a clear message.
- Selectors: the sidebar specs drop the header prefix where a brand class is already unique. A `within` step scopes its steps' locators to one element (a locator with neither selector nor role is that element). Verified: a wrong value inside `within` fails on its own line, a scoped locator doesn't find elements outside the scope, a bare locator outside `within` and `record` inside it are rejected.

## Scenario discovery

Built (2026-10-27): `ScenarioCatalog` is built from one or more assemblies and finds every concrete `Scenario` in them; the tests and the Cli pass the ScenarioTesting assembly, and `SpecLoader.Load` takes the catalog. A scenario's `Name` defaults to its class name in kebab case without "Scenario". `[ScenarioArguments(...)]` creates one scenario per attribute through the constructor, with the values appended to the name, so the wordmark layout scenarios are now `sidebar-branding-wordmark-layout-start` and `-fill` (specs renamed to match). Two scenarios with one name throw when the catalog is built.

- Verified: the Cli lists the ten scenarios under the expected names; 14 of 14 tests pass on four runs. The first run after the build had one failure, which wasn't captured and didn't recur.

## Cleanup

Done (2026-10-28): removed the demo and placeholder specs `branding`, `command-palette`, `trip-edit` and `trips-index`, the `BrandingScenario` and its Cli launch profile; proper ones will replace them. Left: the auth specs on `staff-only`, the sidebar branding specs, and `default` as the Cli's default scenario with no specs. The sections above describe the earlier set.

- Verified: 10 of 10 tests pass in the test project.

## Authorization specs

Done (2026-10-28): `Scenarios/Authz/AuthorizationScenario` (`authorization`) replaces `staff-only`: the panel requires Admin or Agent, Payments and an Audit log sidebar link require Admin, and the home page lists resources instead of the setup checklist. Specs in `Specs/Authz/`: `authz-sign-in` (a deep link goes to sign-in and back), `authz-access-denied` (Viewer), `authz-sign-out` (user menu, Log out, panel asks to sign in again), `authz-agent-permissions` (no Payments or Audit log in the sidebar or home page; Payments directly is denied) and `authz-admin-permissions` (both shown; Payments opens). HTTP rules (401/403, policy combinations, logout schemes) stay in the integration tests.

- Verified: 12 of 12 tests pass on four runs. The first Payments page load takes over a second, so its click has a 5 s timeout.

## Docs captures in the generator

Done (2026-10-28): `docs/DocsSamplesGenerator` runs every spec in its `Captures/` folder after the demos, against the scenarios in its own assembly, and saves the screenshots to the website's `public/images/admin/generated/`, which `CleanOutputFolders` now clears. The generator uses the Web SDK, so it has the static assets manifest the scenario hosts map. `SetupChecklistScenario` (`setup-checklist`) has no resources and the Voyager Travel branding; `home-page-setup-checklist` collapses the first step and draws a box around the checklist, in light and dark. The images are not yet used on the website.

- Verified: the generator run writes both images, checked by eye. The scenario tests were not re-run (the ScenarioTesting project didn't change).
- `annotate` takes `padding` (pixels between the element and the mark, default 4); the checklist capture uses 10. Verified: the regenerated images, by eye. The scenario tests were not re-run.

Open: trip prices show as `3890.0`. Seeding cost if test processes multiply.

## Docs capture placeholders

Done (2026-10-28): docs pages place admin screenshots with `<Capture name alt todo? variants?>` (website `src/components/mdx.tsx`). It shows `/images/admin/generated/{name}.png` (or `{name}-{variant}` per variant, side by side and captioned), with `{name}-dark` in dark mode, sized from `src/generated/captures.json`, which the generator writes after capturing. Until the image exists it shows the `todo` in a dashed box in development and nothing in production. The generator takes `captures` (capture only) and `captures --check` (report only) and reports open placeholders (no spec saves the image) and unused shots (no page shows them) after every run. The `capture-scenario` skill covers the workflow.

- First batch, in `Captures/` folders per page: branding (header, the four logo kinds, the three wordmark layouts), sidebar (resource item, red missing link), environment indicator (stripe, frame, banner) and choices (enum radio group, on a form model since the seeded difficulty is an `int`). Logos are the sandbox SVGs in the generator's `wwwroot/img/`. The setup checklist pages now use `<Capture>` too.
- Open: `sidebar-links` needs a real Razor Page and MVC action in the scenario host.
- Verified: `captures` runs all 14 specs (1 open, 0 unused); every image checked by eye; website `types:check` and `build` pass, `lint` fails only on two untouched files' formatting; the branding and environment indicator pages checked in a dev server, and the open placeholder renders.
- Candidates for later batches: the docs audit of 2026-10-28 (in the conversation, not recorded here) lists about 100 spots; the larger ones need host support (Razor views, pages and partials in the scenario host).

## StellarAdmin CLI

Done (2026-10-28): the generator moved to `tools/StellarAdmin.Cli`, a Spectre.Console.Cli app (help text names it `stellar`) with area-first commands: `docs generate`, `docs check` and `docs clean`, each of `generate` and `clean` with `demos` and `screenshots` subcommands and both on its own. Strict parsing is on, so an option a command lacks is an error. `generate` deletes its own output first unless given `--keep-output`; `--spec <name>` (repeatable) captures only those screenshot specs and keeps the others. Docs captures are now screenshots: `Captures/` is `Docs/Screenshots/`, `CaptureCheck` is `ScreenshotCheck`, and on the website `<Capture>` is `<Screenshot>` and `captures.json` is `screenshots.json`. `DemoGenerator` and `ScreenshotGenerator` each own their output paths, their run and their `Clean`; the commands only parse options and call them. Spectre.Console went from 0.57.1 to 0.57.2, which Spectre.Console.Cli 0.57.2 requires.

- Done (2026-10-28): the scenario CLI folded in as a `scenarios` area, and `tests/StellarAdmin.ScenarioTesting.Cli` was removed. `scenarios list` (a table with each scenario's source), `scenarios start <scenario> [--urls <url>]` and `scenarios run <file|dir>... [--output <dir>]`, which was `--specs` and `--screenshots`. The `scenarios` catalog holds the test and the docs scenarios; `docs generate screenshots` still uses only the docs ones. `run` shows the same progress bar and per-spec host warnings as screenshots and exits 1 if any spec fails. `CollectingLoggerProvider` moved to the CLI root and prints its messages itself. The `scenarios start default` run profile replaces the old project's (port 5210). `sidebar-branding-none` now expects a title starting `Stellar Admin `, since the title comes from the entry assembly and the CLI's is `Stellar Admin Cli`. Verified: the CLI builds; help and the unknown-scenario and missing-path errors; `scenarios run` on the 12 test specs passes (after the title change) and on the 7 branding docs specs with `--output`, which saved the PNGs; `scenarios start default --urls http://127.0.0.1:5207` serves `/stellaradmin` (200); 12 of 12 scenario tests pass.

- Screenshots show a progress bar with the spec being captured. `SpecRunner.RunAsync` and `ScenarioHost.StartLoopbackAsync` take an optional `configureLogging`; the CLI collects each host's warnings and prints them after its spec instead of over the display. Verified: the progress bar and the sidebar-missing-link warning in a pseudo-terminal; 12 of 12 scenario tests pass; the scenario CLI builds.
- Run profiles in `Properties/launchSettings.json`: `docs generate`, `docs generate demos` and `docs generate screenshots`.
- Verified (subcommands): the CLI builds; `docs generate` runs both (14 specs, 1 open, 0 unused); `docs clean screenshots` runs; `--keep-output` parses before and after the subcommand; `--spec` on `demos` is an unknown option. The website came out unchanged once the per-run demo IDs and tokens were discarded.
- Verified: the CLI builds; help, the `--spec` validation errors, `docs generate` (all 14 specs, 1 open, 0 unused), `docs generate screenshots --spec branding-logo-icon`, and `docs clean screenshots` followed by `docs generate screenshots` all run. The screenshots came out byte-identical; the demos differ only by per-run IDs and antiforgery tokens. Website `oxfmt` and `oxlint` pass on `mdx.tsx`. The scenario tests were not re-run (the ScenarioTesting projects didn't change).

## CI

Done (2026-10-28): `ci.yml` and `release.yml` run the scenario tests with `STELLARADMIN_CHROMIUM` set to the runner's preinstalled `/usr/bin/google-chrome`. No spec records, so ffmpeg isn't installed.

- The first CI run failed one click (`authz-sign-in`) at the 1 s timeout on the busy runner; the default step timeout is now 5 s when `CI` is set, 1 s otherwise.
- The second run failed the Payments click at 5 s: paging is off by default, so the Payments index rendered about 13,000 rows (3 MB, 0.8 s locally). Every scenario resource now enables paging, the Payments click lost its 5 s timeout, and the `capture-scenario` skill says to enable paging unless the spec is about an unpaged index. Verified: 12 of 12 tests pass locally with `CI=true`; CI not yet re-run.

## Later

- CI: install ffmpeg once a spec records a clip.
