# Scenario testing

Status: in progress (2026-10-27), on branch `scenario-testing-poc`.

Browser tests of the admin panel driven by **scenarios** (a named admin panel configuration in C#) and **specs** (YAML step lists run against one scenario). The same runner saves screenshots, annotated images and video clips, but the focus is tests; docs captures will probably move into the DocsSamples generator later, so test and capture specs are not split now.

## Built

- `tests/StellarAdmin.Scenarios`: `Scenario`, `ScenarioCatalog`, `ScenarioHost` (TestServer, loopback server and the interactive host share one app), the trips fixture, and the spec runner in `Specs/`.
- Spec steps: `goto`, `expectUrl`, `expectVisible`, `expectText`, `click`, `hover`, `check`, `uncheck`, `fill`, `type`, `select`, `press`, `scroll`, `colorScheme`, `viewport`, `screenshot` (viewport or element), `annotate`, `clearAnnotations`, `spotlight`, `record`, `recordEnd` and `pause`.
- Recording: the whole run is recorded and each `record`…`recordEnd` stretch is cut into an MP4 (and an optional GIF) with a drawn cursor, ripples and pacing options.
- `tests/StellarAdmin.Scenarios.Tests` runs every spec in `Specs/` as a test; outputs save only with `STELLARADMIN_SCREENSHOTS=1`.
- `tests/StellarAdmin.Scenarios.Host`: runs a scenario interactively, or runs specs with `--specs <file|dir> [--screenshots <dir>]`.
- The `capture-scenario` development skill writes, runs and checks a scenario and spec from a plain-English description.

## Verified

- The three specs (`trips-index`, `branding`, `command-palette`) pass in the test project and through the host, with and without outputs.
- Screenshots, annotated images and clips were checked by eye; the skill's run command was checked on `trips-index`.
- Not checked: Playwright's own Chromium (only the system `/usr/bin/chromium`), CI, Windows or macOS.

## Test data

Built (2026-10-27): every scenario runs on the Voyager Travel database, on EF Core with SQLite. The read-only trips fixture was removed; `default` and `branding` use `AddVoyagerTravelResources()` (trips with search, create, edit and delete, destinations and countries).

- `tests/StellarAdmin.Scenarios/VoyagerTravel/` holds the model, `ApplicationDbContext` and seeder from `voyager-travel-generator`, copied as-is by `sync.sh` (the generator stays the single source). The only change is the seed's anchor day, fixed at 2026-10-18 instead of the day it runs, so dates don't move between runs.
- `VoyagerTravelDatabase`: the template is created and seeded once per process in a private in-memory database (about 7 s) and never written. Each app gets its own copy through SQLite's backup API (a few milliseconds) in a named shared-cache in-memory database, held open until the app's services are disposed. `ScenarioHost` adds it to every scenario. Every spec run starts from the same data, and parallel runs share nothing.
- Specs: `trip-edit` changes a trip's title; `trips-index` checks the seeded title is still there.
- Packages added: Bogus 35.6.5, Microsoft.AspNetCore.Identity.EntityFrameworkCore 10.0.11, and Microsoft.EntityFrameworkCore.Sqlite in the scenarios project.

## Login

Built (2026-10-27): ASP.NET Core Identity on the seeded users, added to every scenario by `ScenarioHost` (`Fixtures/ScenarioSignIn.cs`).

- `/scenario/sign-in` lists the users and signs one in by email with Identity's cookie, no password; it is Identity's login path, and `/scenario/access-denied` its access-denied path. Log out in the user menu signs out of Identity's cookie.
- Specs sign in with a top-level `user: <email>` key, or a `signIn: <email>` step to switch users partway.
- The `staff-only` scenario requires the Admin or Agent role. Specs: `sign-in` (redirect to sign-in and back), `sign-out` (Log out in the user menu), `access-denied` (a Viewer is refused, then an Agent signs in).
- The user menu shows the email as the name: Identity's name claim is the user name, which the seed sets to the email, and `DisplayName` isn't a claim.

## Verified

- All seven specs pass in the test project (8 of 8 tests with the TestServer branding test, run in parallel) and through the host. `trips-index` and the `command-palette` clip were checked by eye on the new data.

Open: trip prices show as `3890.0`. Seeding cost if test processes multiply.

## Later

- Move docs captures into the DocsSamples generator.
- CI: install Chromium and ffmpeg, run the scenario tests.
