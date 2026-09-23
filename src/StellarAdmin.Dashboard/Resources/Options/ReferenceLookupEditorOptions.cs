using StellarAdmin.Dashboard.Resources.Editors;

namespace StellarAdmin.Dashboard.Resources.Options;

/// <summary>
///     Configures a reference lookup editor.
/// </summary>
public sealed class ReferenceLookupEditorOptions
    : EditorOptions,
        IFieldEditorOptions<ReferenceLookupEditor>
{
    internal Type? LookupProviderType { get; private set; }

    /// <summary>
    ///     Selects the provider that supplies lookup values.
    /// </summary>
    public void UseLookup<TProvider>()
        where TProvider : class, IReferenceLookupProvider => LookupProviderType = typeof(TProvider);
}
