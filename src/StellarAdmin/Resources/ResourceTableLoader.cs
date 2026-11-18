namespace StellarAdmin.Resources;

// Lists the rows of a table with a handler, with their total for the pager
internal delegate Task<ResourceListResult<object>> ResourceTableLoader(
    IServiceProvider services,
    ResourceListRequest request,
    CancellationToken cancellationToken
);
