# Admin panel skill evaluations

Evaluation tasks for the admin panel consumer skills, from the [admin panel skills plan](../../../docs/plans/archive/admin-panel-skills.md). Tasks and grading checks are in [tasks.md](tasks.md).

Each run uses Voyager, a small Razor Pages app with EF Core and SQLite, consuming StellarAdmin as NuGet packages packed from this checkout into a local feed. Agents see only what an application developer would: the packages, their XML documentation and, for the "with skills" runs, the installed skills. They do not see this repository.

## Run

```bash
build/skill-evals/admin-panel/prepare.sh /path/outside/repo/run-1
```

This packs the packages into `run-1/feed`, and creates `run-1/task-1` to `task-6`, each built and ready: `task-1` without StellarAdmin, the others with it installed (`fixtures/installed`). Give each task's prompt to a fresh agent working in its directory, with no access to this repository. For a "with skills" run, install the skills into each task directory first.

Grade against the checks in [tasks.md](tasks.md) by running each app rather than trusting the agent's report: `probe.sh <task-dir> <port> <roles|-> <path>...` builds and runs the app, signs in through `/dev-login` with the given roles, fetches each path and saves the responses for inspection. Record results in the plan.

`fixtures/` holds only the files that differ from `dotnet new webapp`; `prepare.sh` lays them over a fresh template. Do not build the fixtures in place: the repository's `Directory.Build.props` and central package management would apply.
