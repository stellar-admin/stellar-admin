using StellarAdmin.Dashboard.Resources.Editors;

namespace StellarAdmin.Dashboard.Resources.Displays;

/// <summary>
///     Resolves the items a lookup display's value refers to.
/// </summary>
public sealed class LookupDisplayHandler(LookupDisplay display, IServiceProvider services)
    : FieldDisplayHandler<LookupDisplay>(display)
{
    /// <inheritdoc />
    public override async Task<object?> PrepareAsync(
        FieldDisplayContext context,
        CancellationToken cancellationToken
    )
    {
        var items =
            Display.Items
            ?? throw new InvalidOperationException(
                $"LookupDisplay on {context.FieldName} requires UseItems."
            );

        return await items.FindAsync(
            services,
            new FieldEditorContext(context.FieldName, context.Model, context.Value),
            cancellationToken
        );
    }
}
