using Microsoft.Extensions.DependencyInjection;

namespace StellarAdmin.Resources.Builders;

/// <summary>
///     Configures the default text that every resource's pages use.
/// </summary>
public sealed class ResourceLabelsBuilder
{
    private readonly IServiceCollection _services;

    internal ResourceLabelsBuilder(IServiceCollection services) => _services = services;

    /// <summary>
    ///     Configures the default text of the create page.
    /// </summary>
    /// <param name="configure">A delegate that sets the default create page text.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
    public ResourceLabelsBuilder Create(Action<ResourceCreateLabelsBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        configure(new(_services));

        return this;
    }

    /// <summary>
    ///     Configures the default text of the delete confirmation.
    /// </summary>
    /// <param name="configure">A delegate that sets the default delete confirmation text.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
    public ResourceLabelsBuilder Delete(Action<ResourceDeleteLabelsBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        configure(new(_services));

        return this;
    }

    /// <summary>
    ///     Configures the default text of dictionary editors.
    /// </summary>
    /// <param name="configure">A delegate that sets the default dictionary editor text.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
    public ResourceLabelsBuilder DictionaryEditor(Action<DictionaryEditorLabelsBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        configure(new(_services));

        return this;
    }

    /// <summary>
    ///     Configures the default text of the edit page.
    /// </summary>
    /// <param name="configure">A delegate that sets the default edit page text.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
    public ResourceLabelsBuilder Edit(Action<ResourceEditLabelsBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        configure(new(_services));

        return this;
    }

    /// <summary>
    ///     Configures the default text of the index page.
    /// </summary>
    /// <param name="configure">A delegate that sets the default index page text.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
    public ResourceLabelsBuilder Index(Action<ResourceIndexLabelsBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        configure(new(_services));

        return this;
    }

    /// <summary>
    ///     Configures the default text of list editors.
    /// </summary>
    /// <param name="configure">A delegate that sets the default list editor text.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
    public ResourceLabelsBuilder ListEditor(Action<ListEditorLabelsBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        configure(new(_services));

        return this;
    }

    /// <summary>
    ///     Configures the default text of the lookup editor.
    /// </summary>
    /// <param name="configure">A delegate that sets the default lookup editor text.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
    public ResourceLabelsBuilder Lookup(Action<LookupLabelsBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        configure(new(_services));

        return this;
    }

    /// <summary>
    ///     Configures the default text of the sheet that pages load content into.
    /// </summary>
    /// <param name="configure">A delegate that sets the default sheet text.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
    public ResourceLabelsBuilder Sheet(Action<ResourceSheetLabelsBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        configure(new(_services));

        return this;
    }

    /// <summary>
    ///     Configures the default text of the view page.
    /// </summary>
    /// <param name="configure">A delegate that sets the default view page text.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
    public ResourceLabelsBuilder View(Action<ResourceViewLabelsBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        configure(new(_services));

        return this;
    }
}
