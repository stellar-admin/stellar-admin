using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Dashboard.Resources.Options;
using StellarAdmin.TagHelpers;

namespace StellarAdmin.Dashboard.Resources.Builders;

/// <summary>
///     Configures the index rows that expand to show details.
/// </summary>
public sealed class ResourceRowDetailsBuilder<TResource>
{
    private readonly IServiceCollection _services;

    /// <summary>
    ///     Whether details animate open and closed. Animation is always off when the user prefers reduced motion.
    /// </summary>
    /// <remarks>
    ///     Defaults to <c>true</c>.
    /// </remarks>
    public bool Animate
    {
        set => Configure(options => options.Animate = value);
    }

    /// <summary>
    ///     How an expanded row and its details are set apart from the rows around them.
    /// </summary>
    /// <remarks>
    ///     Defaults to <see cref="TableRowDetailEmphasis.Band" />.
    /// </remarks>
    public TableRowDetailEmphasis Emphasis
    {
        set => Configure(options => options.Emphasis = value);
    }

    /// <summary>
    ///     How many rows can be expanded at once.
    /// </summary>
    /// <remarks>
    ///     Defaults to <see cref="TableRowDetailExpandMode.Multiple" />.
    /// </remarks>
    public TableRowDetailExpandMode ExpandMode
    {
        set => Configure(options => options.ExpandMode = value);
    }

    /// <summary>
    ///     Where the content of a row's details starts.
    /// </summary>
    /// <remarks>
    ///     Defaults to <see cref="TableRowDetailInset.Aligned" />.
    /// </remarks>
    public TableRowDetailInset Inset
    {
        set => Configure(options => options.Inset = value);
    }

    /// <summary>
    ///     Whether clicking anywhere in a row toggles its details. Clicks on links, buttons and form controls keep
    ///     their own behavior.
    /// </summary>
    /// <remarks>
    ///     Defaults to <c>false</c>.
    /// </remarks>
    public bool RowClick
    {
        set => Configure(options => options.RowClick = value);
    }

    /// <summary>
    ///     Where the buttons that expand and collapse rows are placed.
    /// </summary>
    /// <remarks>
    ///     Defaults to <see cref="DataGridRowDetailToggle.Leading" />.
    /// </remarks>
    public DataGridRowDetailToggle Toggle
    {
        set => Configure(options => options.Toggle = value);
    }

    internal ResourceRowDetailsBuilder(IServiceCollection services) => _services = services;

    private void Configure(Action<ResourceRowDetailsOptions> configure) =>
        _services.Configure<ResourceOptions<TResource>>(options =>
            configure(options.Index.RowDetails!)
        );
}
