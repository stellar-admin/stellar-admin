namespace StellarAdmin.Dashboard.Areas.StellarAdmin.ViewModels.Internal;

// A resource's create form in a sheet at the given stack level, for the lookup editor whose hidden input has the For
// id. Its fields bind with their own prefix, since the page's form already uses Entity for its IDs and names, and
// each level has its own, since a create form's lookup can open another create form above it.
internal sealed record CreateSheetViewModel(
    ResourceFormPageViewModel Form,
    string For,
    int Level,
    string PostUrl
)
{
    public string BindingPrefix => GetBindingPrefix(Level);

    public static string GetBindingPrefix(int level) => level == 1 ? "Sheet" : $"Sheet{level}";
}
