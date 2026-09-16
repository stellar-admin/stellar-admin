using StellarAdmin.Pro.Resources.Options;

namespace StellarAdmin.Pro.Resources.Builders;

/// <summary>
///     Configures a titled form section.
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
public sealed class FormSectionBuilder<TEntity> : FormContainerBuilder<TEntity>
    where TEntity : class
{
    private readonly FormSectionOptions _options;

    /// <summary>
    ///     Supporting text displayed below the section title.
    /// </summary>
    public string? Description
    {
        get => _options.Description;
        set => _options.Description = value;
    }

    /// <summary>
    ///     The section title.
    /// </summary>
    public string Title
    {
        get => _options.Title;
        set
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(value);

            _options.Title = value;
        }
    }

    internal FormSectionBuilder(FormSectionOptions options)
        : base(options)
    {
        _options = options;
    }
}
