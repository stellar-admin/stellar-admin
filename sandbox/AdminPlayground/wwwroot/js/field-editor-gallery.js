// Field editor gallery: "Skip client validation" turns off the browser's constraint checks for the
// submit that follows, so invalid values reach server validation. The click listener runs before the
// browser validates, and pressing Enter in a field also submits through a click on the submit button.
document.addEventListener(
    "click",
    (event) => {
        const form = event.target.closest?.("button[type='submit'], input[type='submit']")?.form;
        const skip = form?.querySelector("input[type='checkbox'][name$='.SkipClientValidation']");
        if (skip) {
            form.noValidate = skip.checked;
        }
    },
    true
);
