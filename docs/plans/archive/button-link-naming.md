# Button link naming

Status: completed (2026-11-01).

Clean up the inconsistent naming of the Tag Helper that renders an anchor styled as a button (Jerrie, 2026-11-01). The code called it `<sa-button-link>` (`ButtonLinkTagHelper`), while most of the docs and samples called it a link button.

## Decision

The Tag Helper is `<sa-link-button>` (`LinkButtonTagHelper`) (Jerrie, 2026-11-01). `sa-button` is a button; a link button is an `<a>` that looks like one, and "link button" is the name developers reach for. It is the one exception to the rule that a part's link version appends `-link` to the part's tag, recorded in the port-shadcn-component skill. `<sa-button variant="link">` (a button styled as a link) stays as it is (Jerrie, 2026-11-01).

Where it was already called a link button (found 2026-11-01) and so needed no change: the DocsSamples folder and namespace `Pages/LinkButton`, the demo navigation's **Link Button** item, the `LinkButton/*` entries in `DemoGenerator.cs` and `skills.examples.json`, the exported demo files and includes, the website page `content/docs/ui/components/link-button.mdx`, its `meta.json` entry and the prose on the website's conventions and pagination pages.

## Changes

- `ButtonLinkTagHelper.cs` renamed to `LinkButtonTagHelper.cs`, the class and target element renamed, and `PublicAPI.Unshipped.txt` updated.
- Every `sa-button-link` use in the admin panel views, DocsSamples, sandboxes, the scenario testing host and the consumer skills' handwritten references; the generated skill references regenerated.
- Website: the hand-written pages `link-button.mdx`, `tooltip.mdx`, `dropdown-menu.mdx`, `admin/widgets.mdx` and `admin/page-tag-helpers.mdx`; the demo includes regenerated.
- Not changed: the archived plans and the point-in-time prototypes in `sandbox/html/`.

## Verification

2026-11-01:

- `dotnet build StellarAdmin.slnx -c Release`: 0 errors.
- `dotnet test StellarAdmin.slnx -c Release`: 2091 of 2092 passed. The one failure was in `StellarAdmin.ScenarioTesting.Tests`, which passed 23 of 23 when run on its own.
- `stellar skills generate` matched the renamed references; `skills check` passes.
- `docs generate demos` regenerated the website includes. The run also rewrote 32 `public/demo/ui` files with new antiforgery tokens and carousel IDs only; those were restored.
- Website `pnpm types:check` passes. Not rendered in a browser.

## Button versions named role last

Two tags with button and link versions put "button" in the middle, against the role-last rule their link versions follow (Jerrie, 2026-11-01). Renamed: `sa-attachment-button-trigger` to `sa-attachment-trigger-button` (`AttachmentTriggerButtonTagHelper`) and `sa-bubble-button-content` to `sa-bubble-content-button` (`BubbleContentButtonTagHelper`). The marker and sidebar menu button and link tags already follow the rule and were left alone.

Verified 2026-11-01: build 0 errors; `dotnet test StellarAdmin.slnx -c Release` 2092 of 2092 passed; `skills generate` and `skills check` pass; `docs generate demos` regenerated the attachment and bubble includes (token-only `public/demo/ui` churn restored); website `pnpm types:check` passes. Not rendered in a browser.
