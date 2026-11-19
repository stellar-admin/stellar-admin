namespace StellarAdmin.UI;

/// <summary>
///     Specifies how a data grid column group aligns its head over its columns.
/// </summary>
[GenerateDataAttributeText]
public enum DataGridColumnGroupAlign
{
    /// <summary>
    ///     Aligns the head to the start of the group.
    /// </summary>
    Start,

    /// <summary>
    ///     Centres the head over the group's columns.
    /// </summary>
    Center,

    /// <summary>
    ///     Aligns the head to the end of the group.
    /// </summary>
    End,
}
