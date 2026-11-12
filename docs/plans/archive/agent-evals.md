# Agent evaluations: StellarAdmin vs no StellarAdmin

Status: completed (2026-11-12): phases 1 and 2 done; phase 3 stopped after its one-run check, because more runs wouldn't change the conclusion; phases 4 and 5 not started, kept as ideas.

## Conclusion

StellarAdmin makes an agent's build faster and somewhat cheaper, with far less code and a more finished UI, but the build costs little either way. In phase 3 (one run per variant at 4, 10 and 21 entities), the StellarAdmin variants took 2.4 to 4.3 minutes against `razor`'s 4.7 to 7.1, and cost $0.45 to $0.92 against $0.76 to $1.06. The gap narrowed at 21 entities. They didn't use fewer input tokens: `razor` used the fewest at 10 and 21 entities, and `sa-docs`' docs reading used the most. `razor` wrote 1,200 to 1,700 lines against 280 to 1,100, and its 21-entity app had a paging bug. Rubric scores were 3.2 to 3.6 for `razor` against 3.6 to 4.2. The skills over the docs made a smaller difference than StellarAdmin itself: `sa-skills` was cheaper than `sa-docs` at 4 and 10 entities and level at 21. Phases 1 and 2 (2 entities) showed the same pattern at a few cents and about a minute.

Building the panel isn't where the case for StellarAdmin lies, since Sonnet builds a working 21-entity admin from scratch for about a dollar. It lies in the code that's left to own and change afterwards, which phase 4 would measure. The runner in `build/agent-evals` is kept for that or for later reruns.

## Summary

Measure what StellarAdmin saves a coding agent that builds an admin panel: give the same agent the same task three times, with and without StellarAdmin, repeat each several times, and compare checks passed, tokens, time and tool calls. The idea comes from the .NET team's skill results (token use, time and pass rates per skill, published on the [dotnet/skills dashboard](https://dotnet.github.io/skills/)).

The question it answers: when an agent can write an admin site from scratch, why use StellarAdmin? The aim is to show, fairly, whether it's cheaper and faster (decision 8). Phases 1 and 2 measured a small app built once; phases 3 and 4 measure a realistic size and changes after the build, where the difference is expected to be larger.

The first focus is StellarAdmin vs no StellarAdmin. The existing skill evaluations in `build/agent-evals/admin-panel` (six tasks, run by hand for the [admin panel skills](admin-panel-skills.md)) move into the same structure later; they measure what the skills add, a separate and smaller question.

## Decisions

1. Runner: our own script around `claude -p`, building on `prepare.sh` and `probe.sh`, rather than Vally (which runs the Copilot agent and needs a Copilot subscription) or Harbor. Settled 2026-11-12.
2. `razor` builds with Razor Pages, the stack Voyager already uses, without an admin framework. Settled 2026-11-12.
3. `sa-docs` and `sa-skills` start with StellarAdmin already wired in with no resources: the packages referenced, `AddStellarAdmin` with an empty configuration and `MapStellarAdmin("/admin")`, so the panel lives at the same `/admin` the checks use for `razor`. The agent adds the resources. Settled 2026-11-12.
4. Sonnet, 3 runs per variant (9 runs). Settled 2026-11-12.
5. The grader agent is Opus, the same for every run. Settled 2026-11-12.
6. The variants have descriptive codes, `razor`, `sa-docs` and `sa-skills`, instead of B, C and A. Settled 2026-11-12; the earlier run roots' `runs.tsv` and `meta.json` were renamed to match.
7. Checks test only what the prompt asks for, and requirements come from what an admin panel needs, not from StellarAdmin's feature list; anything the prompt doesn't ask for is left to the blind rubric. Checks aren't added or changed after seeing results to favour a variant; if a check changes, every variant is rerun and the change is recorded here. Settled 2026-11-12, when `sa-skills`' default values for new bookings came up as a possible check and were left to the rubric.
8. The headline measures are minutes, cost and input tokens. The checks are a correctness gate: a variant only counts if it passes them. The rubric is secondary and only shows large differences: the grader scores the same UI a point apart from run to run, so rubric means within about 0.5 of each other count as equal. Settled 2026-11-12.

## Variants

All three start from the same Voyager app and get the same prompt. Each variant adds one thing to the previous one:

| Variant | Starting point | Measures |
| --- | --- | --- |
| `razor` | Voyager: Razor Pages, EF Core with SQLite, Customer and Booking models, seed data, `/dev-login`; `CLAUDE.md` says the app is built with Razor Pages | the baseline |
| `sa-docs` | `razor` + StellarAdmin from a local feed, wired in at `/admin` with no resources + a paragraph in `CLAUDE.md` naming StellarAdmin and its docs | `razor` → `sa-docs`: what StellarAdmin brings |
| `sa-skills` | `sa-docs` + the `stellar-admin` and `stellar-admin-setup` skills in the project's `.claude/skills/` | `sa-docs` → `sa-skills`: what the skills bring |

Everything else is fixed: model, reasoning effort, tools (including web access for all three, since `sa-docs` reads the docs), timeouts. The agent sees neither this repository nor the user's own Claude configuration (`CLAUDE.md`, memory, skills). The variant's instructions are in `CLAUDE.md` rather than `AGENTS.md`, since the runner is Claude Code.

The public docs have an Agent Skills page, so an `sa-docs` agent may install the skills itself; `run.sh` records any `SKILL.md` in the app after the run, and the summary shows it.

## The task

One prompt asking for an admin panel for Voyager, written without StellarAdmin vocabulary. Draft scope, drawn from the existing tasks 1 to 3:

- Staff can list, create, edit and delete customers and bookings, at `/admin`.
- The bookings list shows reference, customer name, destination, departure date, status and total; search by reference or customer name; filter by status; sort by departure date and total; 10 per page.
- The booking form chooses the customer from a searchable list; required fields and ranges are validated.
- Only signed-in users with the Staff role can use it.

The prompt is in [build-admin/prompt.md](../../../build/agent-evals/build-admin/prompt.md). The checks test behaviour only, never API names, so they apply to all three variants:

- Deterministic, in `voyager/checks/checks.mjs` (D1 to D5): Staff can open `/admin`; anonymous users and users without Staff are refused there and on every admin page linked from it; the seeded bookings appear. A refusal is anything other than a 200 under `/admin`, so an app with its own sign-in page under `/admin` would wrongly fail; check by hand.
- Interactive (search, filters, sorting, paging, creating and editing): the URLs and markup differ per implementation, so a grader agent works through a written [checklist](../../../build/agent-evals/build-admin/checklist.md) (L1 to L8, F1 to F5) in a browser against the running app and reports pass, partial or fail per item.
- Judged: screenshots of the list and form scored 1 to 5 against a [rubric](../../../build/agent-evals/build-admin/rubric.md) (R1 to R5: visual quality, list and form usability, narrow screens, navigation), blind to the variant.

Measured per run: checks passed, tokens, cost, time and turns (from the result event of `claude -p --output-format stream-json`), tool calls (from the transcript), lines of code written (leaving out `wwwroot/lib/`).

## Runner

See the [README](../../../build/agent-evals/README.md). `setup.sh <root>` packs StellarAdmin into a feed and installs chrome-devtools-mcp for the grader. `run.sh <root> <variant>` lays the variant over a fresh `dotnet new webapp` with its own `nuget.config` (no local feed for `razor`), builds it, commits it to a new Git repository and runs `claude -p` with Sonnet in it, keeping the transcript. `grade.sh <root> <id>` builds and starts the app with a fresh database, runs the deterministic checks and an Opus grader with a headless Chromium. `run-all.sh` interleaves the variants and grades each run; `summarize.mjs` tabulates the runs and the means per variant. Runs have opaque ids, so neither agent nor grader sees the variant in a path.

Isolation: both agents run in a bubblewrap sandbox with an empty home directory and a cleared environment, with the tools (mise) read-only and only their own workspace writable. They can't see this repository, other checkouts, other runs or `~/.claude`, `~/.agents`; the network is shared. Credentials come from `claude setup-token` (`~/.config/stellaradmin-agent-evals/claude-token`, `CLAUDE_CODE_OAUTH_TOKEN` or `ANTHROPIC_API_KEY`), since the interactive login's credentials file can't be shared with the sandbox safely.

## Layout

```
build/agent-evals/                  # was build/skill-evals/
  README.md
  lib.sh                            # the sandbox, credentials and paths
  setup.sh                          # a run root: the feed and the grader's browser tool
  pack-feed.sh                      # packs StellarAdmin from this checkout into a local feed (from prepare.sh)
  run.sh                            # one agent run of one variant
  grade.sh                          # checks, grader agent, screenshots, result files
  run-all.sh                        # n runs of each variant, graded, then the summary
  summarize.mjs                     # tables per run and per variant
  voyager/
    base/                           # overlay on dotnet new webapp (was fixtures/fresh), plus CLAUDE.md
    package/                        # sa-docs and sa-skills: package reference, Program.cs wired at /admin, CLAUDE.md docs pointer
    configured/                     # the skill tasks (was fixtures/installed)
    checks/                         # probe.sh (the skill tasks) and checks.mjs (D1 to D5)
  admin-panel/                      # the skill tasks: README.md, tasks.md, prepare.sh
  build-admin/
    prompt.md                       # given to the agent verbatim
    checklist.md                    # the grader agent's interactive checks
    rubric.md                       # the screenshot rubric
    task.env                        # the starting app, database, rubric and seeded pattern
  voyager-travel/                   # phase 3: base/ and package/, overlays on dotnet new webapp
  travel-4/, travel-10/, travel-21/ # phase 3 tasks: prompt.md, checklist.md, task.env
  travel-rubric.md
```

## Phases

### Phase 1: fixtures, task and one run per variant

- [x] Settle the runner, `razor`'s stack, the starting point, the model and the grader (decisions 1 to 5).
- [x] Move `build/skill-evals` to `build/agent-evals`, `fixtures/fresh` to `voyager/base` and `fixtures/installed` to `voyager/configured`; keep `prepare.sh` and the six tasks working; update `docs/repos/skills.md` and the README. The fixtures stay overlays on `dotnet new webapp` rather than complete apps: a complete app was only needed for Vally.
- [x] Add `voyager/package` and `pack-feed.sh`.
- [x] Write the `build-admin` prompt, the deterministic checks, the grader checklist and the rubric.
- [x] Write `run.sh`, `grade.sh` and the summary; settle isolation.
- [x] Create the agent credentials (`claude setup-token`).
- [x] Run each variant once and check the grading by hand against the app. Checkpoint.

### Phase 2: full runs

- [x] Run each variant 3 times and record checks passed, tokens, cost, time, tool calls and lines written in this plan. Checkpoint. Run with the phase 1 task unchanged, decided at the phase 1 checkpoint.
- [x] Rerun the 3 runs per variant with the locale fix (`~/agent-eval-runs/phase2-en-us`) and record them; the first phase 2 runs stay below as the record of the locale fault. The batch was interrupted when the driving session restarted: one `sa-skills` agent was killed after 55 seconds (exit 143; moved to `excluded.tsv`, not counted) and the last two runs never started, so the remaining `sa-docs` run and two `sa-skills` runs were run separately. From then on `lib.sh` passes the environment, credentials included, through the sandbox's process environment instead of `bwrap --setenv` arguments, which `ps` showed.

### Phase 3: a realistic size

Phases 1 and 2 used 2 entities, where every variant finishes in about two minutes for well under a dollar, too small to show a difference worth having. Real admin panels have 10 to 20 entities. Hand-written pages grow with each entity; StellarAdmin configuration grows by a few lines each. Whether that widens the gap is the hypothesis to measure, not a result.

- Starting point: the Voyager Travel model and seed data from `tests/VoyagerTravel` (21 entities: trips, departures, itinerary days, destinations, cities, countries, bookings, travellers, payments, suppliers, invoices, guides, reviews and others), synced from the sample app generator, in place of the 2-entity Voyager app. Same three variants, `/admin`, `/dev-login` and Staff role.
- Prompt: an admin for the entities staff manage, with requirements from what an admin panel needs (lists with search and sorting, create, edit and delete, related records chosen from lists, validation, access), written without StellarAdmin vocabulary and fixed before any run (decision 7).
- Checks: the deterministic access checks across every admin page, and a grader checklist for a sample of entities chosen before the runs.
- Measures: minutes, cost and input tokens (decision 8), plus output tokens, since output is priced about five times input and `razor` wrote 3 to 4 times `sa-skills`' output in phase 2.

Settled 2026-11-12: only the entities staff manage are in scope; a series of sizes, to show how cost grows with size; 3 runs per variant and size, with the 90 minute agent timeout.

None of the 21 entities is a pure join table: `BookingAddOn` is a line item with a quantity and price, `TripTag` is a named tag, and the many-to-many links (trip tags, guide languages, accommodation amenities) have no entities of their own. Five are children of another record: `BookingPassenger`, `BookingAddOn` and `Payment` of a booking, `ItineraryDay` of a trip and `RoomAllocation` of a departure.

Also settled 2026-11-12:

- Scope: all 21 entities, each with its own list and forms. Child records aren't required to be edited inside their parent: that would be a design choice that could favour one approach.
- The series, all on the Voyager Travel data, fixed before any run:
  - 4 entities: Trips, Departures, Travellers, Bookings.
  - 10 entities: those 4 plus Countries, Cities, Destinations, Guides, Payments and BookingPassengers.
  - 21 entities: all.
- The phase 2 app isn't the smallest size: it's a different, simpler app, so its numbers wouldn't line up with these. 3 sizes × 3 variants × 3 runs is 27 runs, plus grading; the one-run-per-variant check measures the time per run first.

- [x] Settle the open questions.
- [x] Add the Voyager Travel starting point for each variant, the prompt, the checklist sample and the rubric. `voyager-travel/base` and `package` are the starting apps (`VoyagerTravel`, the same `/dev-login`; the StellarAdmin variants wired in at `/admin` with no resources). `run.sh` copies the model and seeder from `tests/VoyagerTravel` and adds a convention storing decimals as double, as the Voyager app does for its totals, since SQLite can't order by decimal. One prompt per size (`travel-4`, `travel-10`, `travel-21`), the same requirements for every kind of record: 25 per page, related records and amounts readable, search, a status filter where there's a status, sorting, create, edit and delete with validation, related records chosen from searchable lists, Staff only. The checklists sample trips, bookings and travellers at every size, guides, cities and payments from 10, and suppliers, trip tags and supplier invoices at 21 (12, 16 and 20 items), with facts read from the seeded database (7,162 bookings, 3,000 travellers, 70 trips, unique search terms). The scripts take a task: `task.env` names the starting app, the database, the rubric and the seeded pattern for the HTTP checks (`VB-10dddd`), and `runs.tsv` and the summary carry the task.
- [x] One run per variant and check the grading by hand. Checkpoint. Results under Verification. Every app failed F5 (deleting a traveller) because of the starting app, not the agents: see there. Not fixed in the evals, since no more runs followed; the lasting fix is a proposed task for the sample app generator.
- [ ] ~~The agreed runs per variant; record the results here. Checkpoint.~~ Not run (2026-11-12): the check already showed the pattern, and 27 more runs would only narrow small differences in minutes and cents.

### Phase 4: changes after the build

An admin panel is built once and changed often, so the cost of a change matters as much as the cost of the build. Each change starts from a variant's finished, graded app, with a fresh agent that hasn't seen the build. It has to read the existing code: about 600 lines of pages in `razor` against about 110 lines of configuration in the StellarAdmin variants.

- Changes: two or three typical admin maintenance tasks, chosen and written before any run, for example adding a field to bookings and showing it in the list, the filter and the form. Not chosen for StellarAdmin features (decision 7).
- Checks: the change works, and the earlier checks still pass, so a change that breaks something counts against it.
- Measures: minutes, cost and input tokens per change (decision 8).
- `run.sh` needs a mode that starts from an existing run's workspace instead of a fresh app.

Not started (2026-11-12). Open questions:

4. Which changes?
5. Starting from which apps: the phase 2 rerun apps (ready now), phase 3's, or both?
6. Runs per variant and change.

- [ ] Settle the open questions.
- [ ] Add the change prompts and checks, and the start-from-a-run mode.
- [ ] One run per variant and change; check by hand. Checkpoint.
- [ ] The agreed runs; record the results here. Checkpoint.

### Phase 5: the skill tasks (later)

Not started (2026-11-12).

- [ ] Convert the six tasks in `tasks.md` to the same runner on `voyager/configured`, compared `sa-docs` vs `sa-skills`.

## Verification

### Phase 1

2026-11-12, without agent credentials:

- `setup.sh` packed the three packages and installed chrome-devtools-mcp 1.13.0.
- `run.sh <root> sa-docs` (then called C) with a dummy token prepared and built the workspace, committed it, and started `claude -p` in the sandbox (`claude-sonnet-5-5`, bypass permissions, no MCP servers), which failed on authentication as expected; `meta.json` and `numstat.txt` were written.
- In the sandbox, `~/projects`, `~/.claude` and `~/.agents` don't exist, the environment holds only the variables `lib.sh` sets, and `dotnet` 10.0.400 and `claude` run.
- `grade.sh` on that untouched `sa-docs` app: D1 passed (Staff gets `/admin`), D2 to D5 failed as expected (no authorization, no resources yet); the app was stopped and port 5310 freed. The grader failed on authentication. Chromium and chrome-devtools-mcp start in the sandbox (a headless screenshot and an MCP `initialize` handshake).
- `summarize.mjs` on a synthetic run.
- `admin-panel/prepare.sh` after the move: packed the feed and built all six tasks.

2026-11-12, one run per variant (`run-all.sh ~/agent-eval-runs/phase1 1`, 15 minutes in all, Sonnet 5.5 agents, Opus grader):

| Run | Variant | Checks | Rubric | Minutes | Cost | Input | Output | Turns | Tool calls | Lines |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| run-a21ec7 | `razor` | 18/18 | 3.0 | 1.5 | $0.25 | 234k | 13k | 8 | 7 | 552 |
| run-ce1d8c | `sa-docs` | 18/18 | 4.0 | 1.9 | $0.26 | 539k | 8k | 27 | 26 | 109 |
| run-092ab9 | `sa-skills` | 18/18 | 4.2 | 1.0 | $0.18 | 305k | 5k | 11 | 9 | 114 |

- What the agents did: `razor` wrote Razor Pages under `Pages/Admin` (Bootstrap, 13 files). `sa-docs` read 13 docs pages with WebFetch, then configured about 110 lines in `Program.cs`. `sa-skills` loaded the `stellar-admin` skill first and wrote `Admin/AdminResources.cs`. None opened a browser; all three tested with curl and said which parts they hadn't tried.
- Grading checked by hand: the deterministic results come from real refusals (401 and 403 for `razor` and `sa-docs`; redirects to `/Account/Login` and `/Account/AccessDenied`, which don't exist, for `sa-skills`) and 21 to 36 crawled admin pages. The grader took 114 to 129 turns per run and named the exact references, counts and totals it saw; its descriptions match the screenshots (`razor`'s table overflowing to the right at 390 pixels, hiding Status and Total; `sa-docs` and `sa-skills` in the admin panel's layout).
- Grading costs about $1.60 per run, against $0.18 to $0.26 for the agent.
- The task doesn't separate the variants on checks: Sonnet passes everything in every variant. The differences are in lines written (5×), rubric (3.0 against 4.0 and 4.2) and, less clearly, time and tool calls. `sa-docs`' docs reading doubled its input tokens.
- Product finding: StellarAdmin's form labels use the raw property name ("DepartureDate") while the index header reads "Departure Date"; both StellarAdmin runs lost rubric points for it. Proposed as a follow-up task.

### Phase 3 check

2026-11-12, one run per variant and size (`~/agent-eval-runs/phase3-check`, Sonnet 5.5 agents, Opus grader). All 9 agents ended normally (exit 0) and every app built. The batch took 1 hour 22 minutes, 8 to 12.5 minutes per run including grading.

| Run | Task | Variant | Checks | Rubric | Minutes | Cost | Input | Output | Tool calls | Lines | Not passed |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| run-5dfb14 | travel-4 | `razor` | 16.5/17 | 3.2 | 4.7 | $0.76 | 979k | 41k | 17 | 1705 | F5 |
| run-92a3b0 | travel-4 | `sa-docs` | 16.5/17 | 4.0 | 2.8 | $0.46 | 1033k | 14k | 32 | 280 | F5 |
| run-66247f | travel-4 | `sa-skills` | 16/17 | 3.6 | 3.5 | $0.45 | 965k | 14k | 18 | 369 | F1 F5 |
| run-3caf1c | travel-10 | `razor` | 20.5/21 | 3.2 | 4.9 | $0.79 | 1084k | 43k | 18 | 1236 | F5 |
| run-6a7f3d | travel-10 | `sa-docs` | 20.5/21 | 3.8 | 3.4 | $0.69 | 1863k | 21k | 43 | 478 | F5 |
| run-b566ec | travel-10 | `sa-skills` | 20.5/21 | 3.8 | 2.4 | $0.53 | 1162k | 17k | 20 | 551 | F5 |
| run-cf946d | travel-21 | `razor` | 24/25 | 3.6 | 7.1 | $1.06 | 1174k | 61k | 15 | 1527 | L1 F5 |
| run-d21daf | travel-21 | `sa-docs` | 24.5/25 | 4.2 | 4.3 | $0.89 | 2338k | 30k | 43 | 785 | F5 |
| run-436e62 | travel-21 | `sa-skills` | 24.5/25 | 4.2 | 4.2 | $0.92 | 2005k | 34k | 26 | 1124 | F5 |

- F5 (create, edit and delete a traveller) was partial in all 9 runs for the same reason, and it is a fault of the starting app. `ApplicationDbContext` sets every required foreign key in the model's namespace to `Restrict`, and that includes the ownership key of the owned `Traveller.Address`. EF then refuses to delete any traveller ("The association between entity types 'Traveller' and 'Traveller.Address#Address' has been severed…"), and every app's `app.log` shows that exception. `razor` showed the exception page; StellarAdmin showed "This record couldn't be deleted. Try again." Excluding ownership keys from that loop (`!foreignKey.IsOwnership`) would fix it. The model is synced from the sample app generator, so the lasting fix belongs there.
- L1 for `razor` at 21 is a real fault: the list handler takes `int page`, which Razor Pages binds to its reserved `page` route value, so every pager link shows page 1.
- F1 for `sa-skills` at 4 is a fair partial: the departure picker shows only trip titles, so departures of the same trip can't be told apart. The other StellarAdmin runs show the dates.
- The grader's notes match its screenshots. Grading cost $1.84 to $2.76 per run (133 to 183 turns, 3 to 5 minutes).
- At every size, the StellarAdmin variants took less time than `razor` (2.4 to 4.3 minutes against 4.7 to 7.1) and cost less at 4 and 10 entities ($0.45 to $0.69 against $0.76 to $0.79). At 21 entities the costs were close ($0.89 and $0.92 against $1.06). Input tokens were not lower: `razor` used the fewest at 10 and 21 entities, and `sa-docs` the most (1.9M and 2.3M, with 43 tool calls against `razor`'s 15 to 18). `razor` wrote 2 to 6 times the lines. These are single runs, so they're only an indication.

### Phase 2 rerun

2026-11-12, 3 runs per variant with `LANG=en_US.UTF-8` in the sandbox (`~/agent-eval-runs/phase2-en-us`; Sonnet 5.5 agents, Opus grader). The killed `sa-skills` run is excluded (see the phase 2 checklist). These are the results to use; the first phase 2 run below is kept as the record of the locale fault. Means per variant:

| Variant | Checks | Rubric | Minutes | Cost | Input | Output | Tool calls | Lines |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `razor` | 100% | 2.9 | 1.8 | $0.28 | 340k | 14k | 10 | 604 |
| `sa-docs` | 100% | 4.1 | 1.9 | $0.33 | 807k | 9k | 27 | 105 |
| `sa-skills` | 100% | 4.0 | 0.8 | $0.17 | 255k | 4k | 8 | 114 |

Rubric means per item (R1 visual quality, R2 list, R3 form, R4 narrow screens, R5 navigation):

| Variant | R1 | R2 | R3 | R4 | R5 |
| --- | --- | --- | --- | --- | --- |
| `razor` | 3.0 | 3.7 | 3.7 | 2.0 | 2.0 |
| `sa-docs` | 4.0 | 5.0 | 4.0 | 3.3 | 4.0 |
| `sa-skills` | 4.0 | 4.7 | 4.0 | 3.7 | 3.7 |

Checked:

- Every run passed all 18 checks; the `razor` customer edit bug of the first phase 2 run didn't recur.
- The locale fix worked: no run worked around the currency format (8 of 9 did before). One `razor` run still pins en-US for its money column, by choice.
- Every `sa-docs` run read 10 or 11 docs pages; every `sa-skills` run set new-booking defaults with `UseFactory` and no `sa-docs` run did.
- All agents exited normally (exit code 0).

Findings:

- `razor` → `sa-docs` (StellarAdmin): about 5.8× fewer lines (604 against 105) and a higher rubric (2.9 against 4.1), most of all on navigation (2.0 → 4.0), the list (3.7 → 5.0) and narrow screens (2.0 → 3.3). Effort without the skills is about the same time and a little more cost than `razor` ($0.33 against $0.28), with 2.4× the input tokens and nearly 3× the tool calls, spent reading the docs.
- `sa-docs` → `sa-skills` (skills): 3.4× fewer tool calls, 58% less time, 68% fewer input tokens and 48% lower cost, with the same quality (4.0 against 4.1). The 0.1 comes from one `sa-skills` run scored 3.6: for the same StellarAdmin UI (the table scrolling sideways in its card at 390 pixels, no sidebar highlight on create and edit pages) the grader gave 3 in that run and in two `sa-docs` runs and 4 in the others. Without that run `sa-skills` averages 4.2. `sa-skills` is also the cheapest and fastest variant overall: against `razor`, 40% lower cost, less than half the time, 25% fewer input and 70% fewer output tokens, and 5× fewer lines.
- Spread: `sa-docs` ranged from 19 to 35 tool calls and 326k to 1,356k input tokens; the other variants varied much less. Three runs per variant remains a small sample.

### Phase 2

First run, with the locale fault described below. 2026-11-12, 3 runs per variant, interleaved (`run-all.sh ~/agent-eval-runs/phase2 3`, 48 minutes in all, Sonnet 5.5 agents, Opus grader). Means per variant:

| Variant | Checks | Rubric | Minutes | Cost | Input | Output | Tool calls | Lines |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `razor` | 98% | 3.1 | 1.9 | $0.30 | 379k | 16k | 11 | 586 |
| `sa-docs` | 100% | 3.8 | 2.3 | $0.34 | 810k | 10k | 31 | 106 |
| `sa-skills` | 99% | 4.0 | 1.2 | $0.19 | 314k | 5k | 9 | 118 |

Rubric means per item (R1 visual quality, R2 list, R3 form, R4 narrow screens, R5 navigation):

| Variant | R1 | R2 | R3 | R4 | R5 |
| --- | --- | --- | --- | --- | --- |
| `razor` | 3.0 | 4.0 | 3.7 | 2.0 | 3.0 |
| `sa-docs` | 4.0 | 4.0 | 4.0 | 3.7 | 3.3 |
| `sa-skills` | 4.0 | 4.7 | 4.0 | 3.7 | 3.7 |

Costs are Claude Code's API-equivalent figures; on a subscription the runs count against its limits instead.

Every non-pass checked by hand:

- Two of three `razor` runs (F5 partial): editing a customer saves nothing. The form renders its fields through a partial with the customer as its model, so the inputs lack the `Customer.` prefix that `TryUpdateModelAsync(customer, "Customer", ...)` binds from; the page redirects to the list as if saved. A real bug.
- One `sa-skills` run (L2 partial): totals showed "¤450.00". A harness fault, not the agent's: the sandbox set `LANG=C.UTF-8`, which .NET maps to the invariant culture. 8 of the 9 runs noticed it while testing and spent work on a culture setting (all but that `sa-skills` run). Fixed afterwards: the sandbox now sets `LANG=en_US.UTF-8` (checked: `en-US $450.00`). The phase 2 numbers include this extra work in every variant.

Findings:

- Checks are at the ceiling for all three variants; this task separates them on code written, quality and effort, not on passing.
- `razor` → `sa-docs` (StellarAdmin): about 5.5× fewer lines, rubric 3.1 → 3.8, the biggest gaps at narrow screens (2.0 → 3.7) and visual quality (3.0 → 4.0), and no data-binding bugs. Without the skills, StellarAdmin costs more effort: `sa-docs` read 12 docs pages in each run, with twice `razor`'s input tokens, three times its tool calls and slightly more time.
- `sa-docs` → `sa-skills` (skills): a third of `sa-docs`' tool calls, about half the time, 60% fewer input tokens and 45% lower cost, and a slightly higher rubric (4.0). `sa-skills` was also the fastest and cheapest variant overall.
- Three runs per variant is a small sample; `sa-docs`' runs ranged from 25 to 40 tool calls and 554k to 1,150k input tokens. Absolute times are short (one to three minutes), so time differences matter less than tokens and tool calls.- `sa-skills` wrote about 12 more lines than `sa-docs` on average: all three `sa-skills` runs set defaults for a new booking with `UseFactory` (a generated reference and today's date, as the skill's common mistakes teach), which no `sa-docs` run did (its date starts at 01/01/0001; the rubric, not a check, noticed); two of them moved the configuration into its own class with extension methods; two added the locale workaround, against one `sa-docs` run.
