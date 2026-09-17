# Release verification

Run the **Release verification** workflow in GitHub Actions, select the product branch or tag, and enter a package version using SemVer without a leading `v` or build metadata.

The workflow shows individual steps for version validation, checkout and commit recording, Node/.NET setup, tool restore, client dependency installation, solution build, packing, package validation, each test suite, consumer reference checking, the consumer smoke test, and artifact upload. All five libraries and their symbols are built; package validation includes Source Link URL reachability.

`build/smoke-test.sh` contains the consumer test: it creates a temporary Razor Pages app, installs the newly packed libraries using an isolated cache and source mapping, and checks registration, rendered components, and static assets. Release orchestration lives directly in [the workflow](../.github/workflows/release.yml).

The workflow uses the selected product revision as its only checkout and uploads ten verified package/symbol files. It has read-only repository permissions and no publishing job. It must be present on the default branch for manual dispatch. [GitHub dispatch documentation](https://docs.github.com/en/actions/reference/workflows-and-actions/events-that-trigger-workflows#workflow_dispatch)

## Publishing cutover (step 4)

The workspace still owns the active publisher. Keep its workflow, smoke script, and tool manifest until the cutover; the copied product tooling is maintained here. The website remains separate and is not a release input.

Before enabling product publishing:

1. Run the new workflow on GitHub and review its SHA and artifacts. Confirm live product tags and NuGet package versions, keeping historical tags unchanged.
2. Verify the product `nuget-org` environment, protection rules, `NUGET_USER` availability, and a NuGet trusted policy for owner `stellar-admin`, repository `stellar-admin`, workflow `release.yml`, and environment `nuget-org`. A dry run cannot verify OIDC exchange. [NuGet trusted publishing](https://learn.microsoft.com/en-us/nuget/nuget-org/trusted-publishing)
3. Inspect tag/release rules and downstream automation before replacing the cross-repository App token with `GITHUB_TOKEN`. Confine `contents: write` and `id-token: write` to the publish job; tags/releases created with this token generally do not trigger other workflows. [GitHub workflow triggering](https://docs.github.com/en/actions/how-tos/write-workflows/choose-when-workflows-run/trigger-a-workflow)
4. Add serialized publication with cancellation disabled. Replace the old `sort -V` guard with SemVer precedence and an explicit recovery mode bound to the original version, SHA, and artifacts. Recovery must reject a conflicting tag or source revision and reuse the verified artifacts after a partial push; it must not rebuild different package bytes under the same published version.
5. Preserve the publish allowlist of `StellarAdmin.Core` and `StellarAdmin.TagHelpers`; all five remain verification inputs. Publish the downloaded verified artifacts, tag only the built product SHA, and attach only published packages and symbols to the release.
6. Disable workspace publishing before enabling the product publisher. Repository-scoped concurrency cannot coordinate two active publishers. Retire old App credentials and NuGet trust only after checking their remaining uses, then perform the next separately authorized release.

External settings have not been changed by this migration. See the [consolidation plan](../docs/plans/workspace-retirement.md) for actual verification and the remaining distribution/retirement work.
