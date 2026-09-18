# Native scrolling carousel

## Code audit — 2026-09-18

Current status: **completed**. Carousel tag helpers, `sel-carousel.ts`, structural/theme CSS, samples, generator entries and website documentation exist. This is native scrolling, not an unimplemented Embla integration.

This assessment uses the current checkout; earlier status, paths, permissions and verification notes below describe historical sessions. Runtime/browser and hosted release checks were not rerun for this documentation audit.

## Historical record

Status: completed. Last updated: 2026-09-05.

## Agreed design and authorization

Jerrie approved implementation using a lightweight Lit web component after reviewing the CSS-only proposal. This supersedes the earlier zero-JavaScript control design. The authorized scope covered the OSS component, samples, website documentation, and consumer references. Jerrie subsequently authorized committing and pushing all five repositories; those pushes completed successfully.

Use CSS overflow and scroll snapping for movement, with a light-DOM sel-carousel handling native invoker commands, previous/next disabled state, and optional indicators. Preserve shadcn root/content/item/previous/next composition and Base UI outline icon-small chevrons. Add sa-carousel-indicators for generated position buttons. Keep all slide content accessible; intercept keyboard navigation only when the viewport itself is focused. Without scripts or invoker support, hide controls and retain native scrollbars and scrolling.

Support horizontal/vertical layouts, responsive item widths, reduced motion, resize/content changes, and multiple instances. Navigation steps between distinct clamped start-aligned item positions; multiple final items can share one position. Use gap via --carousel-spacing in place of Embla's negative-margin/padding track. No autoplay, looping, Embla API, mouse grab/drag, or CSS pseudo-element controls. Normal buttons permit arbitrary icon content, existing button sizes/variants, and translated aria-label values. Generated IDs use per-render uniqueness to support loops.

## Implementation and validation

- OSS: six Tag Helpers plus orientation enum, sel-carousel, structural CSS, Voyager Travel samples.
- Pro: demo exporter registrations and city photo asset export.
- Website: generated demos/snippets and hand-written API/compatibility documentation.
- Workspace/skills: curated examples and generated references.
- Validate builds, client type checking, rendered Razor, commands, resizing/mutations, keyboard/RTL/vertical behavior, JavaScript-disabled scrolling, theme/mobile/dark screenshots, website checks, and skills drift. Record unavailable browser/assistive-technology checks. Preserve port 5205.

## Research and compatibility

The [Base UI carousel](https://ui.shadcn.com/docs/components/base/carousel) still uses Embla. Its public anatomy is root, content, items, previous, and next. The [Base source](https://raw.githubusercontent.com/shadcn-ui/ui/refs/heads/main/apps/v4/registry/bases/base/ui/carousel.tsx) uses an overflow-hidden viewport, a flex track with negative spacing, nonshrinking full-basis items, and outline icon buttons. The [Radix registry](https://ui.shadcn.com/r/styles/new-york-v4/carousel.json) confirms the structural and rounded-button styling, although Base uses chevrons and icon-sm while Radix uses arrows and an explicit size-8. Base is the visual reference.

[Chrome's introduction](https://developer.chrome.com/blog/carousels-with-css) explains that native scroll buttons move approximately 85% of the viewport, with snapping influencing the final position. They are not an Embla slide-index API. Mandatory snapping and scroll-snap-stop can constrain traversal; exact one-item behavior must be verified for responsive widths rather than promised prematurely.

As checked on 2026-09-05, MDN's source compatibility data lists Chrome 135+ and corresponding Chromium support for [scroll buttons](https://raw.githubusercontent.com/mdn/browser-compat-data/main/css/selectors/scroll-button.json) and [scroll markers](https://raw.githubusercontent.com/mdn/browser-compat-data/main/css/selectors/scroll-marker.json), with Firefox and Safari unsupported. Logical inline/block directions are supported; next/prev button keywords are not. Use feature queries, not browser sniffing.

[Chrome 154 beta](https://developer.chrome.com/blog/chrome-154-beta), announced September 2, introduces explicit links and tabs marker modes with different focus and accessibility behavior. These modes are not a stable-browser prerequisite we can assume today. The [Overflow 5 editor's draft](https://drafts.csswg.org/css-overflow-5/) specifies links as the default, but shipped older implementations differ. Feature-detect the explicit links syntax before generating native markers for a multi-item content carousel.

The [Chrome accessibility follow-up](https://developer.chrome.com/blog/accessible-carousel) documents fixes after the initial release and distinguishes one-slide experiences from collections with multiple interactive items. Native controls still need accessibility testing. Do not indiscriminately make off-screen items inert: that changes content availability and is inappropriate for a freely browsable collection. The [WAI carousel pattern](https://www.w3.org/WAI/ARIA/apg/patterns/carousel/) informs region/group naming, slide labels, focus behavior, and control names; the proposed fallback is a scrolling collection with navigation links, not a simulated ARIA tab widget.


## Final verification and handoff

Implementation completed on 2026-09-05. The initial verification below predates the review follow-ups; the final commit and push status is recorded at the end of this document.

- OSS library and final sample builds pass with zero warnings/errors. The machine has SDK 10.0.400, so builds ran from the workspace instead of using the unavailable nested 10.0.100 pin; no SDK pins changed. Initial sample rebuilds exposed existing nullable warnings, and initial restores carried cached NU1900 warnings; refreshed OSS restore metadata cleared the latter. The final skills run still reported a NuGet metadata warning, but generation and drift checking passed.
- Client JavaScript build and TypeScript --noEmit pass. No dependency added. The shipped JavaScript bundle grows from 43,097 to 47,644 bytes, about 1,158 extra bytes with gzip in this environment.
- Sandboxed Tailwind subprocesses sometimes returned success with zero-byte CSS. Final CSS was built outside the sandbox, and all eight local and exported theme files were explicitly checked for carousel rules and non-empty content. ThemeGenerator was not run.
- Chromium interaction checks pass for all five Razor compositions: previous/next, endpoint disabling, direct indicator jumps, rapid smooth navigation, disconnect/reconnect, viewport arrow navigation without intercepting input keys, dynamic insertion/removal, empty/single-item sets, responsive indicator counts, unique root IDs, and absence of unresolved sa-* elements. JavaScript-disabled content scrolls and controls are hidden. Native scrollbars are suppressed only when a ready carousel has both direct navigation buttons.
- Accessibility-tree checks confirm carousel and navigation names. Mobile layout has no horizontal page overflow. Desktop/mobile/light/dark screenshots were reviewed; fixes from this review include a one-pixel item inset to preserve card rings and correctly centered vertical controls. All eight themes and the built docs page were captured. Forced-colors controls remain visible. These checks are not a screen-reader audit; Firefox, Safari, and screen-reader testing were unavailable on this machine.
- Website lint, type checking, and production build pass. The built carousel docs route renders and its exported demo initializes successfully. Existing demo content was preserved from the pre-generation snapshot, with only the two required shared asset fingerprint references changed in each existing HTML file. New carousel demos/snippets and updated shared assets are retained.
- SkillsGenerator regenerated 48 component references and its --check reports no drift. The carousel reference includes a preserved hand-written composition/behavior section.

Intentional deviations from upstream: native scroll/snap viewport replaces Embla's overflow-hidden wrapper and translated inner track; CSS gap replaces negative margin/item padding; one-pixel item padding protects card outlines; grid positioning aligns arrows with the viewport when indicators are present; only focused-viewport keyboard events are handled, preserving input/link behavior. Navigation controls are buttons because they issue commands, not anchor links. Optional indicators represent distinct reachable positions. No Embla options/plugins/API, infinite looping, autoplay, or synthetic mouse dragging. Browser-native scroll timing replaces Embla physics. All slide content stays accessible; there is no inferred live selected-slide announcement.

Initial implementation changes by repository (before the review follow-ups and final commits): stellar-admin has the new Carousel Tag Helpers, orientation enum, Lit component, CSS/import registration, and five DocsSamples compositions/navigation; stellar-admin-pro has five exporter registrations; website has the new page/navigation, five snippets and demos, shared assets and fingerprint updates; skills has the new generated reference and component index; workspace has the curated example registration and this completed plan/index entry.

Screenshots are available in /tmp/carousel-desktop.png, /tmp/carousel-mobile.png, /tmp/carousel-dark.png, /tmp/carousel-website.png, and /tmp/carousel-theme-<theme>.png. Browser verification scripts and build logs are also under /tmp/carousel-*. These are session artifacts, not required product files. No remaining implementation work; cross-browser and assistive-technology checks remain release validation limitations.

The samples server on 5206, static docs server on 5207, and Chromium instance on 9223 started for this task have been stopped; Jerrie's port 5205 was untouched.

## Coding review follow-up

Jerrie's review established multiline indented XML comments, blank lines between logical method sections, mandatory braces around `if`/`else`, and typed contexts for child configuration. These rules are recorded in the shared C# and XML conventions, the OSS development guide, and the porting skill. Carousel now publishes an internal sealed `CarouselContext` containing its ID and resolved orientation; content and navigation children consume it with `GetContext`, without parent-instance properties or repeated defaults. Missing context produces a clear invalid-nesting exception. A clean library build and targeted checks for supplied/generated IDs, both orientations, navigation targets, custom content, nested context isolation, and missing-context diagnostics passed.

## RTL scope follow-up

Jerrie decided against presenting RTL as a supported feature of this individual component when it is not a library-wide capability. Removed the dedicated RTL sample, exporter registration, website example/support claim, and the corresponding generated demo/snippet. The existing direction-aware scrolling and control behavior remain as defensive handling of native browser directionality; they are not a separate RTL support commitment. The earlier five-demo counts and RTL validation notes above describe the original implementation; four demos remain after this change.

## Photo demo follow-up

Replaced the cards in all four carousel samples with the approved local city photographs. Each figure has a CSS gradient overlay showing the city, country, and a linked photographer credit with Unsplash attribution. Added descriptive image alternatives, intrinsic image dimensions, lazy loading, and explicit figure widths to prevent aspect-ratio/min-height overflow. The sample index uses full-width sections, and the responsive two-slide example has a wider container so the photos and credits remain readable.

The docs generator now exports the four city JPEGs and rewrites their image URLs using its fixed-asset list. Refreshed carousel HTML, source snippets, and consumer references; unrelated generated content was preserved from a fresh pre-generation snapshot with only shared asset references updated. These changes were included in the final commits and pushes recorded below.

Verified in Chromium at 1280px and 390px: all four carousels initialize, all 16 photographs load at their expected resolution, all 16 credit links exist, overlays fit within the photographs, no page overflow occurs, and navigation works in each exported demo. Inspected desktop/mobile screenshots and captured light/dark versions. Sample build passed with 10 existing nullable warnings; consumer reference drift check passed. Temporary sample, static-file, and browser processes were stopped; port 5205 was untouched.

Website lint, type checking, and production build also passed.

## Final repository status

Jerrie authorized committing and pushing all changed repositories. The following commits were pushed successfully to `origin/master`, and all five working trees were clean immediately afterward:

| Repository | Commit | Contents |
| --- | --- | --- |
| Workspace | `288c394` | Carousel record, curated examples, and coding conventions |
| stellar-admin | `2778aa6` | Carousel implementation and four photo demos with overlay credits |
| stellar-admin-pro | `9c8ad05` | Carousel export registrations and city photo assets |
| website | `0a87b0ee` | Carousel documentation, generated demos, and shared assets |
| skills | `9e7f714` | Carousel component reference |

The city image README was removed at Jerrie's request before these commits; attribution remains in every demo photo overlay. TypeScript logical-section spacing is now explicitly recorded in the OSS development guide alongside the existing C# and XML conventions. The workspace documentation closeout is a subsequent commit authorized by Jerrie. No implementation work remains; the browser and assistive-technology validation limitations above still apply.
