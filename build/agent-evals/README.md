# Agent evaluations

Measures what StellarAdmin saves a coding agent building an admin panel, from the [agent evaluations plan](../../docs/plans/archive/agent-evals.md). The same Claude Code agent gets the same task, [build-admin](build-admin/prompt.md), in three variants of the Voyager app:

- **`razor`:** Razor Pages, no StellarAdmin ([voyager/base](voyager/base)).
- **`sa-docs`:** StellarAdmin wired in at `/admin` with no resources, and its docs named in `CLAUDE.md` (base plus [voyager/package](voyager/package)).
- **`sa-skills`:** `sa-docs` plus the `stellar-admin` and `stellar-admin-setup` skills in the project's `.claude/skills/`.

A task is a directory with `prompt.md`, `checklist.md` and a `task.env` naming its starting app, database, rubric and a pattern the seeded data shows in the admin:

- [build-admin](build-admin/prompt.md): the Voyager app (`voyager/`), customers and bookings.
- [travel-4](travel-4/prompt.md), [travel-10](travel-10/prompt.md) and [travel-21](travel-21/prompt.md): the Voyager Travel app (`voyager-travel/`), with 4, 10 or all 21 kinds of record in scope. Its model and seeder are copied from `tests/VoyagerTravel` at run time, with one convention added (decimals stored as double, since SQLite can't order by decimal). They share [travel-rubric.md](travel-rubric.md).

The skill evaluation tasks from the admin panel skills work are in [admin-panel](admin-panel/README.md) and still run by hand.

## Run

Linux only: the agent and the grader run in a [bubblewrap](https://github.com/containers/bubblewrap) sandbox that hides the home directory (this repository, other checkouts, and Claude's user configuration, memory and skills) apart from the run's own workspace. It needs `bwrap`, `chromium`, `node`, `dotnet` and `claude`, and credentials for `claude -p`: run `claude setup-token` and save the token to `~/.config/stellaradmin-agent-evals/claude-token`, or set `CLAUDE_CODE_OAUTH_TOKEN` or `ANTHROPIC_API_KEY`.

```bash
build/agent-evals/setup.sh ~/agent-eval-runs/one      # packs StellarAdmin into a feed, installs the grader's browser tool
build/agent-evals/run-all.sh ~/agent-eval-runs/one 3   # 3 runs of each variant of build-admin, each graded, then the summary
build/agent-evals/run-all.sh ~/agent-eval-runs/one 3 "razor sa-docs sa-skills" travel-10   # another task
```

The run root must be outside this repository. `run.sh <root> <variant> [task]` and `grade.sh <root> <run-id>` do one run or one grading (`PREPARE_ONLY=1 run.sh ...` stops before the agent, to check a starting app); `node summarize.mjs <root>` prints the tables again. Runs have opaque ids; `runs.tsv` maps them to variants and tasks. Each run directory keeps the agent's transcript (`agent.jsonl`), the lines it changed (`numstat.txt`), its workspace and `grade/`. The agent is Sonnet (`AGENT_MODEL`), with a 90 minute limit (`AGENT_TIMEOUT`); the grader is Opus (`GRADER_MODEL`).

## Before rerunning

The results so far, with their dates and the model versions they ran on, are in the [archived plan](../../docs/plans/archive/agent-evals.md). Check these before comparing a rerun with them:

- **The models move.** `AGENT_MODEL` and `GRADER_MODEL` default to the `sonnet` and `opus` aliases, so a later rerun uses newer models. To compare StellarAdmin versions rather than models, set both to the model ids of the earlier runs (`claude-sonnet-5-5` and `claude-opus-5-5`). The run also packs StellarAdmin from the current source.
- **The travel checklists depend on the seed data.** They name exact counts, records and search terms (7,162 bookings, `VB-100531`, `Treutel`), read from the database `tests/VoyagerTravel` seeds. That model is synced from the sample app generator, so after a sync, seed a database with `PREPARE_ONLY=1 run.sh` and check the facts before running. A stale fact fails its item in every variant.
- **Deleting a traveller fails in the Voyager Travel app.** `ApplicationDbContext` sets the owned `Address`'s ownership key to `Restrict` with every other required key, so EF refuses the delete and F5 is partial in every variant. Fix the model first, or expect it.
- **Time and cost.** A travel run takes 8 to 12.5 minutes with grading, and 27 runs take about 4 to 5 hours. The agent costs $0.45 to $1.06 per run and grading $1.85 to $2.75, at API prices. On a subscription these count against its usage limits instead.
- **Credentials.** Create a new token with `claude setup-token` for each batch, and revoke it and delete the file afterwards.

## Grading

`grade.sh` builds the app and runs it on port 5310 in the sandbox, with a freshly seeded database, then:

1. [checks.mjs](voyager/checks/checks.mjs) checks access and data over HTTP: Staff can open `/admin`; anonymous users and users without Staff are refused there and on every admin page linked from it; the seeded bookings appear.
2. An Opus grader works through the [checklist](build-admin/checklist.md) in a headless Chromium, through [chrome-devtools-mcp](https://github.com/ChromeDevTools/chrome-devtools-mcp), and scores the [rubric](build-admin/rubric.md) from its screenshots. It sees only the running app, not the source or the variant.

The summary counts a pass as 1 and a partial as 0.5, and leaves vendored `wwwroot/lib/` files out of the lines written. Check the grading by hand against the app before trusting a new checklist or rubric.

`voyager/base`, `voyager/package` and `voyager/configured` hold only the files that differ from `dotnet new webapp`; the scripts lay them over a fresh template. Don't build them in place: the repository's `Directory.Build.props` and central package management would apply.
