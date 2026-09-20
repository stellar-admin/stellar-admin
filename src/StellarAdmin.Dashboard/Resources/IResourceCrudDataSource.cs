namespace StellarAdmin.Dashboard.Resources;

/// <summary>
///     Provides index, create, edit, and delete operations for a resource.
/// </summary>
public interface IResourceCrudDataSource<TResource>
    : IResourceDataSource<TResource>,
        IResourceCreateHandler<TResource>,
        IResourceEditHandler<TResource>,
        IResourceDeleteHandler<TResource> { }
