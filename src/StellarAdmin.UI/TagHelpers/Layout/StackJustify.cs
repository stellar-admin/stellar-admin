namespace StellarAdmin.UI.TagHelpers;

/// <summary>
///     Controls how a <c>&lt;sa-stack&gt;</c> distributes its children along the main axis.
/// </summary>
public enum StackJustify
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

internal static class StackJustifyExtensions
{
    extension(StackJustify justify)
    {
        public string GetClass() =>
            justify switch
            {
                StackJustify.Center => "sa-stack-justify-center",
                StackJustify.Start => "sa-stack-justify-start",
                StackJustify.End => "sa-stack-justify-end",
                StackJustify.SpaceBetween => "sa-stack-justify-between",
                StackJustify.SpaceAround => "sa-stack-justify-around",
                _ => "sa-stack-justify-start",
            };
    }
}
