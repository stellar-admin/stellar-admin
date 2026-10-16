namespace StellarAdmin.Areas.StellarAdmin.ViewModels;

internal sealed class ResourceLookupQuery
{
    public string? Field { get; set; }

    public string? For { get; set; }

    public string? Form { get; set; }

    public int Level { get; set; }

    public string[] Selected { get; set; } = [];

    public bool SelectedOnly { get; set; }

    public int Skip { get; set; }

    public string? Term { get; set; }
}
