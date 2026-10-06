namespace StellarAdmin.Dashboard.Areas.StellarAdmin.ViewModels.Internal;

// A resource's create form in the shared sheet, for the lookup editor whose hidden input has the For id. Its fields
// bind with their own prefix, since the page's form already uses Entity for its IDs and names.
internal sealed record CreateSheetViewModel(
    ResourceFormPageViewModel Form,
    string For,
    string PostUrl
)
{
    public const string BindingPrefix = "Sheet";
}
