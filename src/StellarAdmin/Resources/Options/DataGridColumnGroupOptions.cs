using StellarAdmin.UI;

namespace StellarAdmin.Resources.Options;

/// <summary>
///     Stores the configuration of a group of data grid columns, whose head spans its columns.
/// </summary>
public sealed class DataGridColumnGroupOptions
{
    /// <summary>
    ///     Gets how the group's head aligns over its columns.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the alignment. The default is
    ///     <see cref="DataGridColumnGroupAlign.Center" />.
    /// </value>
    public DataGridColumnGroupAlign Align { get; internal set; } = DataGridColumnGroupAlign.Center;

    /// <summary>
    ///     Gets the group that contains the group.
    /// </summary>
    /// <value>
    ///     The containing group, or <see langword="null" /> when the group isn't nested.
    /// </value>
    public DataGridColumnGroupOptions? Parent { get; }

    /// <summary>
    ///     Gets the group's head text.
    /// </summary>
    public string Title { get; }

    internal DataGridColumnGroupOptions(string title, DataGridColumnGroupOptions? parent)
    {
        Title = title;
        Parent = parent;
    }
}
