---
name: embed-website-components
description: Embed working StellarAdmin components directly into the React/TanStack website using generated HTML, a shared runtime and scoped CSS. Use when adding or maintaining inline homepage demonstrations; not for ordinary iframe docs demos or designing new library components.
---

# Embed website components

Read the [website guide](../../../docs/repos/website.md) and the maintained [inline example workflow](../../../docs/design/inline-website-examples.md). The implementation and usage example are in `../website/src/components/inline-example/`; the temporary exporter is `../website/scripts/export-inline-example.mjs`.

Use the retained wrapper and generated assets when they fit the requested page. No route currently consumes them. Add a route or change the homepage only within the current task; retaining this infrastructure does not authorize restoring the experimental examples route.

- Render trusted exported HTML with the same deterministic instance ID on server and client. Keep React responsible for the wrapper and StellarAdmin responsible for its descendants. Use distinct IDs for repeated placements.
- Link the scoped stylesheet from the consuming route and load the browser runtime through the wrapper after hydration. Do not import the browser IIFE into SSR, load full document styles globally, or wrap the current runtime in Shadow DOM: the experiment confirmed document-wide trigger lookups fail across that boundary.
- The current exporter supports the selected collapsible/dialog exports and Observatory only. Extend its parser-based transformations at source, regenerate with `pnpm examples:export`, and review the outputs. Refresh the asset manifest after docs regeneration changes the runtime filename. Website builds consume checked-in outputs and do not run .NET or the exporter.
- For new examples, inspect IDs and reference attributes, CSS selectors, sample utilities and any custom scripts. Generalizing the exporter or changing the public library's CSS consumption model is separate work; follow the user's scope.
- Validate actual page integration using the checklist in the maintained workflow. When only retaining unused infrastructure, verify the route is absent and homepage bundles do not reference the wrapper, scoped CSS or runtime. Keep shared component-development guidance in the product repository; the public consumer skills repo does not own this website workflow.
