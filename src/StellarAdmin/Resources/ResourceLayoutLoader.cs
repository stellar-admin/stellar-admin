namespace StellarAdmin.Resources;

// Lists a handler layout's items and wraps them in the partial's model, with the links to their pages and to the index
// as it shows
internal delegate Task<object> ResourceLayoutLoader(
    IServiceProvider services,
    ResourceListRequest request,
    ResourceLayoutUrls urls,
    CancellationToken cancellationToken
);
