namespace StellarAdmin.Resources;

// Reads and binds a form field whose value isn't its property's, such as the keys of the entities in a collection. A
// field with a binder is never bound by MVC, so its property needs no setter. Options are cached, so a binder is shared
// by every request and gets services from the context.
internal abstract class FieldBinder
{
    // The value the field's editor shows. Return a value of the type the editor edits even when it's empty, since
    // editors convert their values to its type.
    public abstract object? GetValue(FieldBindingContext context);

    // Applies the posted values to the model, reporting a value that can't be applied with AddError
    public abstract Task BindAsync(
        FieldBindingContext context,
        CancellationToken cancellationToken
    );
}
