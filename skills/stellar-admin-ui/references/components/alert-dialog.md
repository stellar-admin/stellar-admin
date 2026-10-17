---
component: AlertDialog
tags: [sa-alert-dialog, sa-alert-dialog-action, sa-alert-dialog-cancel, sa-alert-dialog-description, sa-alert-dialog-footer, sa-alert-dialog-header, sa-alert-dialog-media, sa-alert-dialog-title]
generated: true
---

# AlertDialog

Renders a modal dialog that asks the user to confirm an important action.

## Tags

| Tag | Description |
|-----|-------------|
| `<sa-alert-dialog>` | Renders a modal dialog that asks the user to confirm an important action. |
| `<sa-alert-dialog-action>` | Renders the button that confirms an alert dialog. |
| `<sa-alert-dialog-cancel>` | Renders the button that cancels an alert dialog. |
| `<sa-alert-dialog-description>` | Renders the description of an alert dialog, shown below the title. |
| `<sa-alert-dialog-footer>` | Renders the footer of an alert dialog, which holds the cancel and confirm buttons. |
| `<sa-alert-dialog-header>` | Renders the header of an alert dialog, which holds the title and description. |
| `<sa-alert-dialog-media>` | Renders an area in an alert dialog for media, such as an icon or illustration. |
| `<sa-alert-dialog-title>` | Renders the title of an alert dialog. |

## Attributes

### `<sa-alert-dialog>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `size` | `AlertDialogSize` | `Default` | `Default`, `Small` |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

> In Razor, enum values are written fully-qualified, e.g. `variant="ButtonVariant.Outline"`.

### `<sa-alert-dialog-action>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `variant` | `ButtonVariant` | `Default` | `Default`, `Destructive`, `Outline`, `Secondary`, `Ghost`, `Link` |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

### `<sa-alert-dialog-cancel>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `variant` | `ButtonVariant` | `Outline` | `Default`, `Destructive`, `Outline`, `Secondary`, `Ghost`, `Link` |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

## Examples

*From `Pages/AlertDialog/_Intro.cshtml`*

```razor
<div class="flex justify-center">
    <sa-button variant="ButtonVariant.Outline" commandfor="--alert-dialog-intro" command="show-modal">
        Show Dialog
    </sa-button>
</div>
<sa-alert-dialog id="--alert-dialog-intro">
    <sa-alert-dialog-header>
        <sa-alert-dialog-title>Discard unsaved changes?</sa-alert-dialog-title>
        <sa-alert-dialog-description>
            You have unsaved changes to your itinerary for Trip #TRV-987. If you continue, your edits will be lost.
        </sa-alert-dialog-description>
    </sa-alert-dialog-header>
    <form method="dialog">
        <sa-alert-dialog-footer>
            <sa-alert-dialog-cancel>Keep editing</sa-alert-dialog-cancel>
            <sa-alert-dialog-action>Continue</sa-alert-dialog-action>
        </sa-alert-dialog-footer>
    </form>
</sa-alert-dialog>
```

*From `Pages/AlertDialog/_JsApi.cshtml`*

```razor
<sa-stack align="StackAlign.Start" class="min-w-md">
    <sa-button variant="ButtonVariant.Destructive" id="--alert-dialog-js-button">
        Remove from Wishlist
    </sa-button>
    <label class="text-sm font-bold">Result:</label>
    <div class="font-mono w-full rounded-md border border-border bg-muted p-3 text-foreground" id="--alert-dialog-js-result">-</div>
</sa-stack>
<sa-alert-dialog id="--alert-dialog-js">
    <sa-alert-dialog-header>
        <sa-alert-dialog-title>Remove from your wishlist?</sa-alert-dialog-title>
        <sa-alert-dialog-description>
            Kyoto will be removed from your saved destinations. You can add it back anytime.
        </sa-alert-dialog-description>
    </sa-alert-dialog-header>
    <form method="dialog">
        <sa-alert-dialog-footer>
            <sa-alert-dialog-cancel>Keep it</sa-alert-dialog-cancel>
            <sa-alert-dialog-action variant="ButtonVariant.Destructive">Remove</sa-alert-dialog-action>
        </sa-alert-dialog-footer>
    </form>
</sa-alert-dialog>
<script type="module">
    (function () {
        const alertDialog = window.stellarAdmin.alertDialog(document.getElementById("--alert-dialog-js"));
        const triggerButton = document.getElementById("--alert-dialog-js-button");
        const resultDisplay = document.getElementById("--alert-dialog-js-result");

        triggerButton.addEventListener("click", async () => {
            const confirmed = await alertDialog.confirmAsync();
            resultDisplay.innerHTML = confirmed ? "Removed from wishlist" : "Cancelled";
        });
    })();
</script>
```
