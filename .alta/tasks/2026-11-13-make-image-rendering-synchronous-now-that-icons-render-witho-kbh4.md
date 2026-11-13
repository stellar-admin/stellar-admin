---
title: "Make image rendering synchronous now that icons render without a tag helper"
kind: improvement
status: pending
created: 2026-11-13
summary: "Since h7j3, the image and image upload rendering chain no longer awaits anything and passes a TagHelperContext it doesn't read; making it synchronous removes about 25 awaits and the unused parameters."
---

## Why
The icon helper from `h7j3` renders without a `TagHelperContext` or `await`. Icons were the only async work in the image components, so these methods are now `async` with no `await`, and/or take a `context` they never read:

- `ImageRendering.RenderImageAsync`, `RenderFallbackAsync`, `RenderLightboxTemplateOnceAsync` (`src/StellarAdmin.UI/TagHelpers/Image/ImageRendering.cs`)
- `ImageUploadTagHelper.Parts.ButtonAsync`, `DropAsync`, `ProgressAsync`, `ImageAsync`, and `Parts.Context` (`src/StellarAdmin.UI/TagHelpers/Image/ImageUploadTagHelper.cs`)
- Their callers `RenderDropzoneAsync`, `RenderCompactAsync`, `RenderAttachmentAsync` and `RenderCellAsync`, which await them about 25 times.

`h7j3` left this chain alone to keep its diff to the icon change; the other components were tidied the same way.

## What to do
Make these methods synchronous (drop the `Async` suffix), remove the unused `context` parameters and `Parts.Context`, and update `ImageTagHelper` and `ImageUploadTagHelper.RenderInput` to match. The output must not change: regenerate the demos into a scratch folder before and after (`STELLARADMIN_WEBSITE_DIR`) and diff them, masking antiforgery tokens and GUID ids.

## Where
`src/StellarAdmin.UI/TagHelpers/Image/`
