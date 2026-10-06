namespace StellarAdmin.Dashboard.Areas.StellarAdmin.ViewModels.Internal;

// The result of a create form in the shared sheet: the key of the new resource, or null when the create handler
// returned none, for the lookup editor whose hidden input has the For id
internal sealed record LookupCreatedViewModel(string For, string? Key);
