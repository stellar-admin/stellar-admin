namespace StellarAdmin.Dashboard.EntityFrameworkCore;

/// <summary>Configures an EF Core reference.</summary>
public sealed class EfCoreReferenceBuilder<TTarget>
    where TTarget : class
{
    internal ReferenceChoicesBuilder<TTarget> ChoiceOptions { get; } = new();

    internal EfCoreReferenceBuilder() { }

    /// <summary>Configures the selectable records.</summary>
    public EfCoreReferenceBuilder<TTarget> Choices(
        Action<ReferenceChoicesBuilder<TTarget>> configure
    )
    {
        ArgumentNullException.ThrowIfNull(configure);
        configure(ChoiceOptions);

        return this;
    }

    // Future phases can add editor selection and non-EF reference sources; references use Select for now.
}
