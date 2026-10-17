---
component: Dialog
tags: [sa-dialog, sa-dialog-description, sa-dialog-footer, sa-dialog-header, sa-dialog-title]
generated: true
---

# Dialog

Renders a modal window over the page.

## Tags

| Tag | Description |
|-----|-------------|
| `<sa-dialog>` | Renders a modal window over the page. |
| `<sa-dialog-description>` | Renders the description of a dialog, below its title. |
| `<sa-dialog-footer>` | Renders the footer of a dialog, which holds its action buttons. |
| `<sa-dialog-header>` | Renders the header of a dialog, which holds the title and description. |
| `<sa-dialog-title>` | Renders the title of a dialog. |

## Examples

*From `Pages/Dialog/_Intro.cshtml`*

```razor
<div class="flex justify-center">
    <sa-button variant="ButtonVariant.Outline" commandfor="--dialog-intro" command="show-modal">
        Open
    </sa-button>
</div>
<sa-dialog id="--dialog-intro">
    <sa-dialog-header>
        <sa-dialog-title>Edit traveller</sa-dialog-title>
        <sa-dialog-description>Update the traveller's details here. Click save when you're done.</sa-dialog-description>
    </sa-dialog-header>
    <sa-field-group>
        <sa-field>
            <sa-label for="--dialog-intro-name">Name</sa-label>
            <sa-input id="--dialog-intro-name" name="name" value="Ibn Battuta"/>
        </sa-field>
        <sa-field>
            <sa-label for="--dialog-intro-username">Username</sa-label>
            <sa-input id="--dialog-intro-username" name="username" value="@@ibnbattuta"/>
        </sa-field>
    </sa-field-group>
    <sa-dialog-footer>
        <sa-button variant="ButtonVariant.Outline" commandfor="--dialog-intro" command="close">
            Cancel
        </sa-button>
        <sa-button commandfor="--dialog-intro" command="close">
            Save Changes
        </sa-button>
    </sa-dialog-footer>
</sa-dialog>
```

*From `Pages/Dialog/_ReturnValue.cshtml`*

```razor
<div class="flex justify-center">
    <sa-button variant="ButtonVariant.Outline" commandfor="--dialog-return-value" command="show-modal">
        Open Alert Dialog
    </sa-button>
</div>
<sa-dialog id="--dialog-return-value">
    <sa-dialog-header>
        <sa-dialog-title>Are you absolutely sure?</sa-dialog-title>
        <sa-dialog-description>
            This action cannot be undone. This will permanently cancel booking VT-48213 and release the reserved seats.
        </sa-dialog-description>
    </sa-dialog-header>
    <form method="dialog">
        <sa-dialog-footer>
            <sa-button variant="ButtonVariant.Outline" type="submit" value="cancel">
                Cancel
            </sa-button>
            <sa-button type="submit" value="confirm" autofocus>
                Continue
            </sa-button>
        </sa-dialog-footer>
    </form>
</sa-dialog>
<script>
    (function() {
        const dialog = document.getElementById("--dialog-return-value");

        dialog.addEventListener("close", () => {
            const cancelled = dialog.returnValue === "" || dialog.returnValue === "cancel";
            if (cancelled) {
                alert("The action has been cancelled");
                return;
            }

            alert("The action has been confirmed");
        });
        dialog.addEventListener("toggle", (e) => {
            // Reset the return value every time the dialog opens to prevent a previous
            // returnValue from being returned when pressing the Esc key
            if (e.newState === "open") {
                dialog.returnValue = "";
            }
        });
    })();
</script>
```
