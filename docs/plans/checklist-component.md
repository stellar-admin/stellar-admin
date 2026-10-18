# Checklist component

Status: proposed (2026-10-18).

Extract the home page's setup checklist (`Areas/StellarAdmin/Views/Home/_SetupChecklist.cshtml` and `_SetupStep.cshtml`) into an `sa-checklist` component in `StellarAdmin.UI`, with `sa-checklist-item` rows. Each item has a status (such as done, to do, warning or optional), a title and description, an action, and details that expand below it. The progress bar and the card around it stay separate, so pages compose them as they need. The home page then uses the component.

Read the current setup checklist and similar `sa-*` components first, then propose the tag helpers, statuses and API to Jerrie before implementing. Follow the component workflow for samples, website docs and consumer references.
