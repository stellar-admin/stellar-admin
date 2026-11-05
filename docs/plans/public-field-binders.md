# Public field binders

Status: open (2026-11-05). Basics only.

`FieldBinder` and `UseBinder()` are public, so an app can read and bind a field with its own code, such as a property without a public setter. Inside StellarAdmin, the EF Core data source uses them for many-to-many fields (`ManyToManyFieldBinder`).

The only way for an app to attach its own binder is `PostConfigure` on `ResourceOptions<TResource>`, looping over the create and edit fields. No builder method exposes it. The website docs section on field binders was removed from the form models page (2026-11-05) while this is open. The consumer reference (`skills/stellar-admin/references/forms.md`) and the namespace table in `skills/stellar-admin/SKILL.md` still mention `FieldBinder`.

Question: do apps need public field binders at all, or can they become internal, with custom data sources handling these properties another way, such as a form model?

To be filled out before implementing.
