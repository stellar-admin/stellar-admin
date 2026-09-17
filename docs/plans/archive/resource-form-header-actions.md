# Resource form header and actions

Status: completed; implemented, verified, and committed.

Last updated: 2026-09-10. Affected repo: `stellar-admin-pro`; this workspace owns the plan. Builds on [form section integration](../oss/form-section.md).

## Intended result

Match the supplied Edit Product reference: a subtle full-width divider below the page title and optional description, and a footer with a quiet red Delete action on the left and outlined Cancel plus primary Save on the right. Apply this consistently to definition-built create/edit forms, including EF resources and Identity users/roles. Retain the Large form container. Layout becomes a form-wide choice, resolved from the form override to the app default (Split); section-level overrides in Pro are removed.

## Starting implementation

The shared `_FormPage.cshtml` renders `sa-page-header` without a divider and a horizontal field containing the submit button and pre/post action slots. EF resource edit and Identity user/role edit views inject a filled destructive Delete button through `post-form-actions`. Each edit view renders `_FormDeleteDialog` outside the main form; that dialog already owns its separate delete POST and confirmation action. There is no form Cancel action today.

## Agreed layout rules

The user revised the earlier three-level cascade: Pro definition-built forms support layout at app and form level only. Remove `FormSectionBuilder.Layout` and `FormSectionOptions.Layout`; preserve nullable create/edit `SectionLayout` overrides and the shared app setting. Resolve the effective layout once for the form and pass it to every section, including nested sections. Update the existing cascade tests and references to the superseded section override API. Standalone OSS `sa-form-section` retains its `layout` attribute for use on custom pages; this change concerns Pro form definitions.

| Effective form layout | Divider below header | Divider above footer | Actions |
| --- | --- | --- | --- |
| Split (default) | Yes | Yes | Delete left; Cancel and Save right |
| Stacked | Yes | Yes | Same |
| Card | No | No | Same |

Use the existing semantic theme border token (`border-border` / `var(--border)`) for both dividers, without hard-coded colors. Button appearance, placement, and behavior are independent of layout. Card sections retain their own component borders.

## Proposed changes

1. For effective Split or Stacked layout only, add a subtle theme-border divider beneath the shared form header, spanning the container's content width with intentional spacing before the fields. Scope this styling to resource forms; do not change every OSS page header. Preserve optional subtitles without adding screenshot-specific product copy.
2. Replace the action field with a responsive footer. Add its matching top border only for effective Split or Stacked layout; Card has neither page divider. Give it a left action region and a right region containing Cancel followed by the existing primary submit button. Keep submit labels and validation behavior unchanged. Use Pro component CSS for the footer structure and spacing.
3. Add an outlined Cancel link styled as a button on both create and edit. Resolve its destination to the current resource's Index route through MVC route generation, including the active area/controller and generic-resource routing. Use deterministic navigation rather than browser history; clicking Cancel must not submit or validate the form. Follow existing localization conventions for its label. Returning to a filtered index or adding unsaved-change prompts is outside this change.
4. Move the built-in Delete trigger into the footer's left region. Use a ghost/text treatment with normal-weight destructive text and no persistent filled background or border; keep an adequate hit target, visible keyboard focus, and a subtle hover state. Reuse the existing label and modal command. Keep the final confirmation inside the dialog visibly destructive and preserve its delete POST behavior.
5. Centralize the built-in Delete trigger in the shared form page, conditional on `Model.Delete`, and remove the duplicated slot injections from EF and Identity edit views. Keep dialog forms outside the main edit form. Preserve the existing pre/post-form-actions slots around the primary actions and verify their registration and rendering; no slot rename is needed for the built-in left action.
6. On narrow screens, allow the footer regions to stack with comfortable spacing and keep Cancel/Save together where they fit. Preserve logical DOM and keyboard order: Delete, Cancel, Save. Create forms retain right-aligned Cancel/submit actions without reserving visible empty content for Delete.

## Validation

- Build Pro, EF integration, Identity, and IdentitySimplePlayground.
- Update layout tests for form → app → Split resolution; verify all sections share the resolved layout, both page dividers appear only for Split/Stacked, and action markup and behavior remain consistent in all three layouts. Verify theme-token border colors in light and dark themes.
- Extend relevant integration checks for Cancel destinations on create/edit, including EF resources and Identity routes; confirm Cancel renders as navigation and Delete only appears when available.
- Verify existing action slots still render once, delete forms remain separate, and confirmation/cancel behavior remains intact. Run the existing integration suite for validation, persistence, and binding regressions.
- Inspect desktop and mobile product forms, plus Identity forms, in the playground. Check Split, Stacked, and Card layouts, header with/without subtitle, action alignment, focus/hover states, dark mode, and absence of overflow. Confirm form Cancel navigates without saving and dialog Cancel leaves the edit page open.

## Implementation and verification

Implemented the shared header/footer and route-aware Cancel action, removed the three duplicated edit-view Delete triggers, and removed the Pro section-level layout API. `_FormPage` resolves form → app once and passes the result to every section. Its `data-form-layout` drives the header/footer border rules. Both borders use `var(--border)` and spacing uses the theme gap tokens. Delete uses the existing Ghost button with normal-weight `var(--destructive)` text, a token-based hover tint, and the existing keyboard focus ring. Cancel uses `sa-linkbutton` to Index with the entity ID cleared. Existing action outlets remain in order around the primary action region. Existing labels use English defaults; there is no localization infrastructure in these form views.

Validation: the playground and complete EF resource integration suite build and pass using installed SDK 10.0.400. Build warnings are existing XML/nullability diagnostics and NU1903 for the existing SQLitePCLRaw dependency. Updated tests cover app/form layout precedence on create/edit and rendered Cancel destinations and Delete availability/treatment for EF products and Identity users/roles. Existing persistence, invalid-submission, reference, binding, and deletion checks pass. CSharpier formatted touched C# files; Pro and workspace whitespace checks pass.

The Pro stylesheet built successfully in the sandbox and contains the new rules (3726 bytes); an earlier elevated CSS-build request timed out in automatic approval review, and the ordinary build provided a successful fallback. Chromium checks covered product create/edit and Identity create forms at 1600px and 390px, all three layouts, and light/dark mode. Layout attributes were switched in the browser to verify CSS; actual app/form resolution was checked separately by the integration suite. Header/footer borders match the active theme token, Card omits both dividers, all layouts have no overflow or nested forms, and edit Delete has weight 400 and a transparent resting background. Cancel navigation and opening/cancelling the product delete dialog work. Keyboard checks verified Delete → Cancel focus order, visible focus indication, and destructive hover tint. Screenshots: `/tmp/pro-actions-{1600,390}-{light,dark}-{split,stacked,card}.png`; logs: `/tmp/pro-actions-{build,tests,browser}.log`.

Committed Pro changes as `2a56e0f` (`Unify resource form layouts and footer actions`) at the user’s request; workspace notes are committed separately. The later playground `Program.cs` import remains uncommitted. OSS and website working trees are clean. No push or publication was requested. The standalone OSS section helper retains its explicit layout attribute. No remaining implementation work for this request.
