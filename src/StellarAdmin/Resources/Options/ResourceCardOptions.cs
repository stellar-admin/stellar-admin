using System.Linq.Expressions;
using StellarAdmin.Diagnostics;

namespace StellarAdmin.Resources.Options;

/// <summary>
///     Stores the configuration of the cards of a cards data layout.
/// </summary>
public sealed class ResourceCardOptions
{
    /// <summary>
    ///     Gets where the row actions sit.
    /// </summary>
    /// <value>
    ///     The placement. The default is <see cref="ResourceCardActionsPlacement.Menu" />.
    /// </value>
    public ResourceCardActionsPlacement ActionsPlacement { get; internal set; }

    /// <summary>
    ///     Gets how the image and the content are arranged.
    /// </summary>
    /// <value>
    ///     The arrangement. The default is <see cref="ResourceCardArrangement.Vertical" />.
    /// </value>
    public ResourceCardArrangement Arrangement { get; internal set; }

    /// <summary>
    ///     Gets the field shown as a badge.
    /// </summary>
    /// <value>
    ///     The field, or <see langword="null" /> for no badge. The default is
    ///     <see langword="null" />.
    /// </value>
    public FormDisplayFieldOptions? Badge { get; internal set; }

    /// <summary>
    ///     Gets the expression that selects the text under the title.
    /// </summary>
    /// <value>
    ///     The expression, or <see langword="null" /> for no description. The default is
    ///     <see langword="null" />.
    /// </value>
    public LambdaExpression? DescriptionField { get; internal set; }

    /// <summary>
    ///     Gets how the fields are arranged.
    /// </summary>
    /// <value>
    ///     The data layout. The default is <see cref="ResourceCardFieldLayout.Rows" />.
    /// </value>
    public ResourceCardFieldLayout FieldLayout { get; internal set; }

    /// <summary>
    ///     Gets the fields, in display order.
    /// </summary>
    public IReadOnlyList<FormDisplayFieldOptions> Fields => MutableFields;

    /// <summary>
    ///     Gets the image configuration.
    /// </summary>
    /// <value>
    ///     The configuration, or <see langword="null" /> for no image. The default is
    ///     <see langword="null" />.
    /// </value>
    public ResourceCardImageOptions? Image { get; internal set; }

    /// <summary>
    ///     Gets the stats, the short values shown in a strip above the fields, in display order.
    /// </summary>
    public IReadOnlyList<FormDisplayFieldOptions> Stats => MutableStats;

    /// <summary>
    ///     Gets the expression that selects the card's title.
    /// </summary>
    /// <value>
    ///     The expression, or <see langword="null" /> when the resource has no title. The default
    ///     is the resource's title.
    /// </value>
    public LambdaExpression? TitleField { get; internal set; }

    internal ConfigurationSource? ArrangementSource { get; set; }

    // Read the title, description and image URL from a record. The check sets the expressions before a card renders
    internal Func<object, object?>? DescriptionValue =>
        field ??= DescriptionField is null ? null : ComputedValue.Compile(DescriptionField);

    internal Func<object, object?>? ImageValue =>
        field ??= Image is null ? null : ComputedValue.Compile(Image.Field);

    internal List<FormDisplayFieldOptions> MutableFields { get; } = [];

    internal List<FormDisplayFieldOptions> MutableStats { get; } = [];

    internal Func<object, object?>? TitleValue =>
        field ??= TitleField is null ? null : ComputedValue.Compile(TitleField);

    internal ResourceCardOptions() { }
}
