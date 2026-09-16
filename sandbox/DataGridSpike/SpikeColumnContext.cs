namespace DataGridSpike;

/// <summary>
///     Published by a spike column before it executes its child content, so nested template
///     tags know which pass is running and where to deposit their captured content.
/// </summary>
public class SpikeColumnContext
{
    public bool Collecting { get; init; }

    public string? HeaderContent { get; set; }

    public string? ItemContent { get; set; }
}
