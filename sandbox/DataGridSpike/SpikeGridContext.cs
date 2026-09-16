namespace DataGridSpike;

/// <summary>
///     Ambient state shared between the spike grid and its columns. A single mutable instance
///     is published via SetContext before any child execution, so all passes see the same object.
/// </summary>
public class SpikeGridContext
{
    public bool Collecting { get; set; } = true;

    public object? Item { get; set; }

    public int RowIndex { get; set; } = -1;

    public List<(string Name, string Header)> Columns { get; } = [];

    public List<string> Cells { get; } = [];

    /// <summary>Spike 2 only: delegates captured by columns during the collect pass.</summary>
    public List<(string Name, Func<Task<string>> Template)> DeferredTemplates { get; } = [];
}
