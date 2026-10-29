using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Diagnostics;
using StellarAdmin.Resources.Options;
using StellarAdmin.UI;

namespace StellarAdmin.Resources.Builders;

/// <summary>
///     Configures how index rows expand to show their details.
/// </summary>
/// <typeparam name="TResource">The type of the resource the index lists.</typeparam>
public sealed class ResourceRowDetailsBuilder<TResource>
{
    private readonly IServiceCollection _services;

    /// <summary>
    ///     Sets a value that indicates whether details animate open and closed.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> if details animate; otherwise, <see langword="false" />. The
    ///     default is <see langword="true" />.
    /// </value>
    /// <remarks>
    ///     Details don't animate when the user prefers reduced motion.
    /// </remarks>
    public bool Animate
    {
        set => Configure(options => options.Animate = value);
    }

    /// <summary>
    ///     Sets the line between an expanded row and its details.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the line. The default is
    ///     <see cref="DataGridRowDetailDivider.Inset" />.
    /// </value>
    public DataGridRowDetailDivider Divider
    {
        set => Configure(options => options.Divider = value);
    }

    /// <summary>
    ///     Sets how an expanded row and its details are set apart from the rows around them.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the emphasis. The default is
    ///     <see cref="DataGridRowDetailEmphasis.Band" />.
    /// </value>
    public DataGridRowDetailEmphasis Emphasis
    {
        set => Configure(options => options.Emphasis = value);
    }

    /// <summary>
    ///     Sets how many rows can be expanded at once.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies how many rows can be expanded. The default
    ///     is <see cref="DataGridRowDetailExpandMode.Multiple" />.
    /// </value>
    public DataGridRowDetailExpandMode ExpandMode
    {
        set => Configure(options => options.ExpandMode = value);
    }

    /// <summary>
    ///     Sets where the content of a row's details starts.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies where the content starts. The default is
    ///     <see cref="DataGridRowDetailInset.Aligned" />.
    /// </value>
    public DataGridRowDetailInset Inset
    {
        set => Configure(options => options.Inset = value);
    }

    /// <summary>
    ///     Sets how a row's details display two or more panels.
    /// </summary>
    /// <value>
    ///     The layout, from <see cref="ResourceRowDetailPanelLayout.Accordion()" /> or
    ///     <see cref="ResourceRowDetailPanelLayout.Tabs()" />. The default is an accordion with
    ///     any number of panels open at once.
    /// </value>
    /// <remarks>
    ///     A layout made with a value that isn't defined is a configuration problem, and the
    ///     default is used.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    ///     The value is <see langword="null" />.
    /// </exception>
    public ResourceRowDetailPanelLayout PanelLayout
    {
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            var source = ConfigurationSource.Capture();
            Configure(options =>
            {
                options.PanelLayout = value;
                options.PanelLayoutSource = source;
            });
        }
    }

    /// <summary>
    ///     Sets a value that indicates whether clicking anywhere in a row toggles its details.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> if a click anywhere in the row toggles its details; otherwise,
    ///     <see langword="false" />. The default is <see langword="false" />.
    /// </value>
    /// <remarks>
    ///     Clicks on links, buttons and form controls keep their own behavior.
    /// </remarks>
    public bool RowClick
    {
        set => Configure(options => options.RowClick = value);
    }

    /// <summary>
    ///     Sets where the buttons that expand and collapse rows are placed.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies where the buttons are placed. The default
    ///     is <see cref="DataGridRowDetailToggle.Start" />.
    /// </value>
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
