---
component: Dialog
tags: [sa-dialog, sa-dialog-description, sa-dialog-footer, sa-dialog-header, sa-dialog-title]
generated: true
---

# Dialog

A modal window overlaid on the page, rendered over a native `<dialog>` element. Open and close it with the Invoker Commands API — a trigger button carrying `commandfor` and `command="show-modal"` or `command="close"`.

## Tags

| Tag | Description |
|-----|-------------|
| `<sa-dialog>` | A modal window overlaid on the page, rendered over a native `<dialog>` element. Open and close it with the Invoker Commands API — a trigger button carrying `commandfor` and `command="show-modal"` or `command="close"`. |
| `<sa-dialog-description>` | The descriptive body text of a dialog, shown beneath the title. |
| `<sa-dialog-footer>` | The footer region of a dialog; typically contains action buttons. |
| `<sa-dialog-header>` | The header region of a dialog; typically contains the title and description. |
| `<sa-dialog-title>` | The title heading of a dialog. |

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
        <sa-dialog-title>Edit profile</sa-dialog-title>
        <sa-dialog-description>Make changes to your profile here. Click save when you're done.</sa-dialog-description>
    </sa-dialog-header>
    <sa-field-group>
        <sa-field>
            <sa-label for="--dialog-intro-name">Name</sa-label>
            <sa-input id="--dialog-intro-name" name="name" defaultValue="Ibn Battuta"/>
        </sa-field>
        <sa-field>
            <sa-label for="--dialog-intro-username">Username</sa-label>
            <sa-input id="--dialog-intro-username" name="username" defaultValue="@@ibnbattuta"/>
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
            This action cannot be undone. This will permanently delete your account from our servers.
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
