namespace StellarAdmin.Areas.StellarAdmin.ViewModels.Internal;

// The rows after an inline save and add another, in grid order: the new row and the saved row, the new row first when
// rows are added at the top
internal sealed record IndexAddAnotherRowsViewModel(
    IResourceIndexPageViewModel First,
    IResourceIndexPageViewModel Second
);
