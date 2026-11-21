namespace StellarAdmin.Resources;

// Lists a handler data layout's items and wraps them in the partial's model, with the links to their pages and to the
// index as it shows
internal delegate Task<object> ResourceDataLayoutLoader(
    IServiceProvider services,
    ResourceListRequest request,
    ResourceDataLayoutUrls urls,
    CancellationToken cancellationToken
);
