namespace StellarAdmin.Resources;

/// <summary>
///     Reads and binds a form field whose value isn't its property's, such as the keys of the
///     entities in a collection.
/// </summary>
/// <remarks>
///     A field with a binder is never bound by MVC, so its property needs no setter. Options are
///     cached, so a binder is shared by every request; get services from
///     <see cref="FieldBindingContext.Services" />.
/// </remarks>
public abstract class FieldBinder
{
    /// <summary>
    ///     Gets the value the field's editor shows.
    /// </summary>
    /// <param name="context">The field and the model to read.</param>
    /// <returns>
    ///     The value, such as a list of keys. Return a value of the type the editor edits even when
    ///     it's empty, since editors convert their values to its type.
    /// </returns>
    public abstract object? GetValue(FieldBindingContext context);

    /// <summary>
    ///     Applies the posted values to the model.
    /// </summary>
    /// <param name="context">The field, the model and the posted values.</param>
    /// <param name="cancellationToken">A token that cancels the operation.</param>
    /// <returns>A task that completes when the values are applied.</returns>
    /// <remarks>
    ///     Report a value that can't be applied with <see cref="FieldBindingContext.AddError" />.
    /// </remarks>
    public abstract Task BindAsync(
        FieldBindingContext context,
        CancellationToken cancellationToken
    );
}
