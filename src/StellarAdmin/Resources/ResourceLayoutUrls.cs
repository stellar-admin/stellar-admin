namespace StellarAdmin.Resources;

// The links of a handler layout's items to their view and edit pages, which return null for an item without one, and
// the link to the index as it shows
internal sealed record ResourceLayoutUrls(
    Func<object, string?> ViewUrl,
    Func<object, string?> EditUrl,
    Func<object?, string> IndexUrl
);
