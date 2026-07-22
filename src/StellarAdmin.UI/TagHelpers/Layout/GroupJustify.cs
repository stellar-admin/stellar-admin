namespace StellarAdmin.UI.TagHelpers;

/// <summary>
///     Controls how a <c>&lt;sa-group&gt;</c> distributes its children along the main axis.
/// </summary>
public enum GroupJustify
{
    /// <summary>Children are centered along the main axis.</summary>
    Center,

    /// <summary>Children are packed toward the start of the main axis.</summary>
    Start,

    /// <summary>Children are packed toward the end of the main axis.</summary>
    End,

    /// <summary>Children are spaced with equal gaps between them and none at the edges.</summary>
    SpaceBetween,

    /// <summary>Children are spaced with equal gaps around each of them.</summary>
    SpaceAround,
}

internal static class GroupJustifyExtensions
{
    extension(GroupJustify justify)
    {
        public string GetClass() =>
            justify switch
            {
                GroupJustify.Center => "sa-group-justify-center",
                GroupJustify.Start => "sa-group-justify-start",
                GroupJustify.End => "sa-group-justify-end",
                GroupJustify.SpaceBetween => "sa-group-justify-between",
                GroupJustify.SpaceAround => "sa-group-justify-around",
                _ => "sa-group-justify-start",
            };
    }
}
