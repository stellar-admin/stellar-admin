# Form section

## Code audit — 2026-09-18

Current status: **completed**. `FormSectionTagHelper`, `FormRowTagHelper`, `StellarAdminFormsOptions` and Dashboard `_FormSection.cshtml`/`_FormRow.cshtml` implement the components and shared integration. `_FormPage.cshtml` resolves the app/form layout; section-level Dashboard overrides were superseded. Samples, generator registrations, website docs and consumer references are present.

This assessment uses the current checkout; earlier status, paths, permissions and verification notes below describe historical sessions. Runtime/browser and hosted release checks were not rerun for this documentation audit.

## Historical record

Status: OSS components, documentation, and Pro form-definition integration implemented. Updated 2026-09-10.

## Agreed design

`FormSectionLayout` has `Stacked`, `Split`, and `Card` values. `Split` is the library default. Split headings appear beside content when the section is at least 48rem wide and above it otherwise. Card headings always appear above content. The OSS `<sa-form-section>` accepts `title`, optional `description`, and nullable `layout`; its children remain ordinary Razor content.

The shared app default is configured through `AddStellarAdmin(stellar => stellar.ConfigureForms(forms => forms.SectionLayout = FormSectionLayout.Split))`. `StellarAdminFormsBuilder` configures `StellarAdminFormsOptions`, available through `IOptions<StellarAdminFormsOptions>`. An omitted tag helper layout inherits this setting.

The agreed future Pro API uses nullable `SectionLayout` on create/edit builders and nullable `Layout` on section builders. Resolve section → form → app and pass the result to the OSS component. `AddSection` remains the structural API; there is no `AddCard` method. This turn only authorizes the OSS component and samples.

## Implemented

Changes are uncommitted in `stellar-admin/`: shared form defaults, the enum and tag helper, component CSS, and `/FormSection` samples linked from the Forms navigation. Samples cover the app default, all three layouts, multiple sibling sections, a narrow container, and an omitted description. Section titles and descriptions are encoded, and each section receives an accessible heading association unless the caller supplies a label.

## Verification

The DocsSamples build (including the library) passes with 10 existing nullable warnings using installed SDK 10.0.400 from the workspace root; the OSS-pinned 10.0.100 is unavailable. Touched C# files were formatted with the restored CSharpier 1.3.0 DLL. All eight theme bundles contain the new component rules. The sandboxed CSS command returned success but emitted empty files, so the final bundles were rebuilt outside the sandbox.

A temporary C# harness passed library defaults, app configuration, explicit overrides, text encoding, child-content preservation, author classes, and heading association checks. Chromium checks covered desktop/mobile Split and Card layouts in nova light and vega dark, plus Stacked, narrow-container, and no-description samples. No horizontal overflow was observed. Visual checks caught and corrected a sample checkbox tag before final verification.

## Form row follow-up (2026-09-10)

The user also authorized the OSS `<sa-form-row>` and DocsSamples examples. Implemented an attribute-free row: each direct child occupies an equal-width column at container widths of 40rem or more, with a single stacked column below that. Children align at the top. Spacing uses the theme's medium gap and can be overridden with a `gap-*` class on the helper. An inner grid allows the row's own width to control its container query without counting rendered children. The component does not emit form or fieldset semantics.

Added `/FormRow` with two-column, three-column, split-section, narrow-container, and single-field examples, plus a Forms navigation link. Replaced the repeated grid utility wrappers in the three FormSection layout examples with `<sa-form-row>`. Existing form-section work was preserved. Pro's `AddRow` integration remains deferred.

Validation: final DocsSamples/library build passes using installed SDK 10.0.400 with the same 10 pre-existing nullable warnings; all eight bundles contain the row rules. Builds ran outside the sandbox to avoid the previously observed empty CSS output. Chromium verified equal widths, top alignment, stacking, rendered children, and no horizontal overflow for all five examples at desktop and mobile sizes in nova light and vega dark. Both complete sample pages were captured and reviewed. Touched C# files were formatted, and `git diff --check` passed.

## Remaining work

Integrate the agreed cascade into Pro when requested. Generated consumer references remain outside the requested scope. The existing screenshots and approved API supplied the design for implementation; no additional prototype approval was required.

## Website documentation follow-up (2026-09-10)

The user authorized generator registration, sample regeneration, website documentation, and a new Form Layout navigation group above Forms in both DocsSamples and the website. Added all ten partials to `stellar-admin-pro/docs/DocsSamplesGenerator/Generator.cs`; the generator exported 406 demos successfully. Website pages at `/docs/tag-helpers/components/form-row` and `/docs/tag-helpers/components/form-section` cover usage, examples, responsive behavior, and API details, including app defaults and per-section overrides. Both navigation groups now contain Form Row and Form Section.

Preserved a pre-generation snapshot of the website's already-modified generated output at `/tmp/form-layout-website-before.tar.gz`. The regeneration changes the shared site CSS fingerprint and theme assets across existing demos; reviewed HTML differences also include SVG attribute ordering. No pre-existing output was reverted. New output includes ten HTML demos and ten MDX snippets. Changes remain uncommitted across OSS (navigation and earlier components), Pro (generator registrations), website (pages, navigation, generated output), and workspace (this record).

Validation: generator succeeded; all new demo/include references resolve and all eight exported bundles contain the new component rules. Website lint, type checks, and production build passed. Chromium loaded both documentation pages, their embedded demos, and the exported Split demo; the navigation group and API sections render and no page overflow was observed. Commands used installed SDK 10.0.400 and elevated local execution where the sandbox prevented the CSS pipeline or pnpm database access.

### Pop-out demo containers

The four pop-out examples now use `<sa-page-container>`: Large for FormRow/InSection and FormSection/Intro, Medium for FormSection/Stacked and FormSection/Card. These exports use `_CleanLayout` so container gutters replace the generic demo padding. Regenerated all 406 demos; source snippets are unchanged. Chromium verified centered 1152px/896px desktop containers and no overflow at 390px mobile width. Existing generated output was snapshotted before regeneration at `/tmp/form-layout-containers-website-before.tar.gz`.

## Commits (2026-09-10)

Committed on `feature/generic-resources`: OSS `f816ed2` (components and samples), Pro `55c3692` (generator registrations), website `e298bea6` (documentation and regenerated demos). Earlier references to uncommitted work describe the implementation checkpoints above. Website staged whitespace checking reports whitespace-only lines in generated HTML from the existing export templates; generated output was committed as emitted. No repositories were pushed.

## Pro integration (2026-09-10)

The user authorized rendering definition-built Pro forms with the OSS layout components. `_FormSection` now renders `sa-form-section`, and `_FormRow` renders `sa-form-row`; the shared partial names remain available for view overrides. Removed Pro's legacy row CSS and per-row column counts. Added nullable `SectionLayout` to create/edit builders and shared form options/view models, plus nullable `Layout` to section builders/options. The form setting travels through ViewData, including nested containers; each section overrides it when configured, otherwise the OSS helper resolves the shared app default (Split).

Configure a form with `resource.Edit(edit => edit.SectionLayout = FormSectionLayout.Card)` or `resource.Create(create => create.SectionLayout = FormSectionLayout.Stacked)`. Set `section.Layout = FormSectionLayout.Split` inside an `AddSection` callback to override that form. Leaving either setting null inherits the next level.

Validation: the EF resource integration suite passes, including added HTTP checks for app/form/section precedence on create and edit inside groups, shared section/row markup, binding prefixes, and existing invalid-submission, persistence, and reference/editor checks. The build includes Pro, Identity, EF integration, and IdentitySimplePlayground and uses installed SDK 10.0.400; package vulnerability lookup emitted NU1900 warnings because NuGet was unreachable, alongside existing code warnings during the full build. CSharpier formatted touched C# files and `git diff --check` passes.

Chromium verified the product create page at 1600px and 390px with five sections and three rows and no horizontal overflow. Additional browser checks switched the rendered layout attributes to Card and Stacked to inspect shared CSS behavior. The existing Medium page container remains: in desktop Split layout its field column is about 558px, so rows correctly stack below their 40rem threshold; Card and Stacked rows have enough space for columns. Screenshots are at `/tmp/pro-form-{1600,390}-{split,card,stacked}.png`. No new width API was added. Consumer reference generation remains outside this task. Changes are uncommitted in Pro and the workspace.

### Wider Pro forms

Changed the shared form page container from Medium to Large at the user’s request. Chromium confirms desktop Split rows now show product name/SKU in two columns and prices in three columns; mobile rows still stack, with no horizontal overflow in Split, Card, or Stacked. The playground build passed with nine existing warnings and no errors using SDK 10.0.400. Changes remain uncommitted in Pro and the workspace.

Committed Pro integration and the wider form container as `1c90967` (`Use shared form sections and rows in resource forms`). Workspace implementation notes are committed separately. These commits have not been pushed.

### Layout scope revision

The user subsequently chose app/form layout settings only for Pro definition-built forms, superseding the section-level override described above. The planned header/footer dividers follow that single effective layout (Split/Stacked only); buttons remain consistent across layouts. See [Resource form header and actions](../../archive/resource-form-header-actions.md) for the completed removal of the Pro section override, shared header/footer actions, and validation; the standalone OSS helper retains its explicit layout attribute.
