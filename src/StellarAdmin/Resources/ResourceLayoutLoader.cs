namespace StellarAdmin.Resources;

// Lists a handler layout's items and wraps them in the partial's model, with a link to the index as it shows. The items
// aren't the resource's records, so they have no view or edit links
internal delegate Task<object> ResourceLayoutLoader(
    IServiceProvider services,
    ResourceListRequest request,
    Func<object?, string> indexUrl,
    CancellationToken cancellationToken
);
