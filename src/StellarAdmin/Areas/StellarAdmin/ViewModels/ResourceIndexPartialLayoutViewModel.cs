namespace StellarAdmin.Areas.StellarAdmin.ViewModels;

/// <summary>
///     Represents the partial view that renders an index page's layout in place of the grid.
/// </summary>
public sealed class ResourceIndexPartialLayoutViewModel
{
    /// <summary>
    ///     Gets the model the partial view receives, a <c>ResourceLayoutModel&lt;TItem&gt;</c>.
    /// </summary>
    public object Model { get; }

    /// <summary>
    ///     Gets the name of the partial view.
    /// </summary>
    public string PartialName { get; }

    internal ResourceIndexPartialLayoutViewModel(string partialName, object model)
    {
        PartialName = partialName;
        Model = model;
    }
}
