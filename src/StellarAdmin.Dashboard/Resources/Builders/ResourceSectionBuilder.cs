using StellarAdmin.Dashboard.Resources.Options;

namespace StellarAdmin.Dashboard.Resources.Builders;

/// <summary>
///     Configures a titled form section and its fields.
/// </summary>
public sealed class ResourceSectionBuilder<TResource> : ResourceFieldsBuilder<TResource>
{
    private readonly string _title;
    private string? _description;

    /// <summary>
    ///     Supporting text displayed below the section title.
    /// </summary>
    public string? Description
    {
        set => _description = value;
    }

    internal ResourceSectionBuilder(string title, Action<Action<IList<FormItemOptions>>> configure)
        : base(configure) => _title = title;

    internal FormSectionOptions Build() => new(_title) { Description = _description };
}
