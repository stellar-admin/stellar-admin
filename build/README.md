# Release workflow

Run the **Release** workflow in GitHub Actions, select the product branch or tag, and enter a package version using SemVer without a leading `v` or build metadata. Leave **publish** unchecked for a dry run. Release orchestration lives directly in [the workflow](../.github/workflows/release.yml), with individual Actions steps.

Every run builds and validates Core, TagHelpers, Dashboard, Dashboard.Identity, and Dashboard.EntityFrameworkCore, runs both test suites and consumer reference checks, tests a temporary consumer app, and uploads all ten package/symbol files. Source Link validation remains enabled. `build/smoke-test.sh` contains the consumer test.

## Publishing

Check **publish** only for an authorized release. The publishing job downloads the verified build artifact and publishes **StellarAdmin.Core** and **StellarAdmin.TagHelpers**, including their symbols. Dashboard packages remain artifacts. The job creates one product tag at the built SHA and a GitHub release containing only the four published package/symbol files. It uses the product `GITHUB_TOKEN`; no cross-repository App is required. Writes and OIDC permissions are confined to this job.

Publishing runs are serialized with cancellation disabled. Version checks use SemVer precedence and inspect product tags plus NuGet versions for both published packages. A normal run must advance the version. A draft release reserves the version before the NuGet push, with a hidden marker binding the source SHA, workflow run ID, and immutable package artifact ID. The release becomes public only after package pushes and release-asset uploads succeed. Dry runs create no draft, tag, or NuGet publication.

## Recovering a failed publish

Use **Re-run failed jobs** on the original workflow run, while its 30-day artifact remains available. This reruns the publishing job against the original verified bytes; it does not rebuild packages. Already accepted packages and symbols are skipped separately, missing release assets are uploaded, and the release is finalized. [GitHub rerun documentation](https://docs.github.com/en/actions/how-tos/manage-workflow-runs/re-run-workflows-and-jobs)

Do not dispatch a fresh run or select **Re-run all jobs** to recover a partial publication. A different run, artifact, or tag SHA is rejected. Recovery also refuses an older version after a newer release exists. Keep the draft's hidden marker intact. If the artifact expired, stop and recover the original bytes through an explicit maintenance decision; do not rebuild an already published version. A failed pre-push run can also leave a reserved draft/tag; review that reservation before deciding to abandon it.

## Required configuration and cutover

The product repository needs:

- A `nuget-org` GitHub environment, with the intended protection rules.
- An environment secret `NUGET_USER` naming the NuGet.org user.
- A NuGet trusted policy for GitHub owner `stellar-admin`, repository `stellar-admin`, workflow `release.yml`, and environment `nuget-org`. Limit its packages to Core and TagHelpers. Core has not previously been published, so allow new package creation as well as new versions. [NuGet trusted publishing](https://learn.microsoft.com/en-us/nuget/nuget-org/trusted-publishing)
- Repository variable `RELEASE_PUBLISHING_ENABLED=true`, set only after configuring NuGet trust and disabling the workspace publisher.

The workspace workflow is replaced with a retirement notice and must be disabled in GitHub before enabling this gate. Retain old credentials/trust until confirming they have no remaining consumers; do not use the workspace for new releases. The separate website is not a release input.

The hosted dry run at [run 35185318228](https://github.com/stellar-admin/stellar-admin/actions/runs/35185318228) passed on product commit `26b039d`. Publishing and OIDC exchange require the next separately authorized release to verify. See the [consolidation record](../docs/plans/workspace-retirement.md) for live configuration and cutover status.
